using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.Diagnostics;

namespace SurveyApp.Utils
{
    /// <summary>
    /// Statická knihovní třída obsahující metody pro komunikaci se Sharepointem
    /// </summary>
    public static class SharePointConnector
    {
        /// <summary>
        /// Přihlašovací metoda
        /// </summary>
        /// <param name="URL">adresa SP site ve které je umístěn dotazník</param>
        /// <param name="user">uživatelské jméno</param>
        /// <param name="password">heslo</param>
        /// <returns>Objekt pomocí které je možné dále provádět dotazování</returns>
        public static Task<ClientContext> LoginAsync(string URL, string user, string password)
        {
            return Task.Run(async () =>
            {
                ClientContext sharePointContext;

                try
                {
                    sharePointContext = new ClientContext(URL);
                    sharePointContext.Credentials = new SharePointOnlineCredentials(user, password);

                    Web web = sharePointContext.Web;
                    sharePointContext.Load(web);
                    await sharePointContext.ExecuteQueryAsync();
                }
                catch (Exception)
                {
                    return null;
                }
                return sharePointContext;
            });
        }

        /// <summary>
        /// Načte seznam všech dostupných SP seznamů
        /// </summary>
        /// <param name="sharePointContext"></param>
        /// <returns>kolekce obsahují jméno a GUID seznamů</returns>
        public static Task<List<ListInfo>> LoadListTitlesAsync(ClientContext sharePointContext)
        {
            return Task.Run(async () =>
            {
                Web web = sharePointContext.Web;
                sharePointContext.Load(web.Lists,
                lists => lists.Include(list => list.Title, list => list.Id));
                await sharePointContext.ExecuteQueryAsync();

                List<ListInfo> result = new List<ListInfo>();
                foreach (List list in web.Lists)
                {
                    result.Add( new ListInfo(list.Id, list.Title));
                }

                return result;
            });
        }

        /// <summary>
        /// Načte veškeré položky zadaného listu
        /// </summary>
        /// <param name="sharePointContext"></param>
        /// <param name="list">objekt odkazující na SP list</param>
        /// <returns>kolekce všech položek daného listu</returns>
        private static Task<List<ListItem>> LoadListItemsAsync(ClientContext sharePointContext, List list)
        {
            return Task.Run(async () =>
            {
                CamlQuery query = CamlQuery.CreateAllItemsQuery(1000);
                ListItemCollection items = list.GetItems(query);
                sharePointContext.Load(items);
                await sharePointContext.ExecuteQueryAsync();

                return items.ToList();
            });
        }
        /// <summary>
        /// Přetížení metody pro vyhledávání pomocí zobrazovaného jména místo objektu
        /// </summary>
        /// <param name="sharePointContext"></param>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        public static async Task<List<ListItem>> LoadListItemsAsync(ClientContext sharePointContext, string listTitle)
        {
            List list = sharePointContext.Web.Lists.GetByTitle(listTitle);

            return await LoadListItemsAsync(sharePointContext, list);
        }
        /// <summary>
        /// Přetížení pro vyhledávání pomocí GUID místo objektu
        /// </summary>
        /// <param name="sharePointContext"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static async Task<List<ListItem>> LoadListItemsAsync(ClientContext sharePointContext, Guid guid)
        {
            List list = sharePointContext.Web.Lists.GetById(guid);

            return await LoadListItemsAsync(sharePointContext, list);
        }

        /// <summary>
        /// Načte definice pololžek jednotlivého listu
        /// </summary>
        /// <param name="sharePointContext"></param>
        /// <param name="list">Objjekt odpovídající Sharpoint listu</param>
        /// <returns>Kolekce informací o jednotlivých sloupcích seznamu</returns>
        public static Task<List<FieldInfo>> GetFieldsAsync(ClientContext sharePointContext, List list)
        {

            return Task.Run(async () =>
            {
                FieldCollection fieldCollection = list.Fields;
                sharePointContext.Load(fieldCollection);
                var t = sharePointContext.ExecuteQueryAsync();
                t.Wait();

                // odfiltrování systémových sloupců, které ani není možné vyplnit
                List<Field> fields = fieldCollection.Where(f => f.Hidden == false).Where(f => f.ReadOnlyField == false).Where(f => f.Group.Equals("Custom Columns") || f.Group.Equals("Vlastní sloupce")).ToList<Field>();
                List<FieldInfo> result = new List<FieldInfo>();
                foreach (Field field in fields)
                {
                    FieldInfo fieldInfo = new FieldInfo();
                    fieldInfo.FieldObject = field;

                    // pokud se jedná o sloupec,k terý odkazuje na jiný sloupec, tak jsou zde načteny hodnoty, na které se je možné odkázat
                    if (field.TypedObject.GetType().Equals(typeof(FieldLookup)))
                    {
                        var fieldlookup = (FieldLookup)field;
                        if (fieldlookup.LookupList != null)
                        {
                            var items = await LoadListItemsAsync(sharePointContext, new Guid(fieldlookup.LookupList));
                            foreach (ListItem item in items)
                            {
                                fieldInfo.LookupItems.Add(new FieldInfoItem(item.Id, (string)item.FieldValues["Title"]));
                            }
                        }
                    }
                    result.Add(fieldInfo);
                }
                return result;
            });
        }
        /// <summary>
        /// Přetížení pro získávání definic polí pomocí GUID
        /// </summary>
        /// <param name="sharePointContext"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static async Task<List<FieldInfo>> GetFieldsAsync(ClientContext sharePointContext, Guid guid)
        {
            List list = sharePointContext.Web.Lists.GetById(guid);

            return await GetFieldsAsync(sharePointContext, list);
        }

        /// <summary>
        /// Přetížení pro získávání definic polí pomocí názvu seznamu
        /// </summary>
        /// <param name="sharePointContext"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        public static async Task<List<FieldInfo>> GetFieldsAsync(ClientContext sharePointContext, string listName)
        {
            List list = sharePointContext.Web.Lists.GetByTitle(listName);
            return await GetFieldsAsync(sharePointContext, list);
        }

        /// <summary>
        /// Zapsání uživatelského vstupu na server
        /// </summary>
        /// <param name="sharePointContext"></param>
        /// <param name="listId"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static Task<bool> AddItem(ClientContext sharePointContext, Guid listId, List<FieldInfo> fields)
        {
            return Task.Run(async () =>
            {
                bool result = true;
                List list = sharePointContext.Web.Lists.GetById(listId);

                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem newItem = list.AddItem(itemCreateInfo);

                foreach (FieldInfo f in fields)
                {

                    try
                    {   
                        // každý typ vyžaduje trochu jiný způsob zápisu
                        if (f.FieldObject.TypedObject.GetType().Equals(typeof(FieldLookup)))
                        {
                            newItem[f.FieldObject.InternalName] = f.LookupIndex;
                        }
                        else if (f.FieldType.Equals("Boolean"))
                        {
                            if (f.FieldValue == null) { f.FieldValue = false; }
                            newItem[f.FieldObject.InternalName] = f.FieldValue;
                        }
                        else
                        {
                            newItem.ParseAndSetFieldValue(f.FieldObject.InternalName, f.FieldValueAsString);
                        }
                    }
                    catch
                    {
                        result = false;
                        Debug.WriteLine(f.FieldTitle);
                    }
                }

                newItem.Update();

                await sharePointContext.ExecuteQueryAsync();

                return result;
            });
        }

    }
}
