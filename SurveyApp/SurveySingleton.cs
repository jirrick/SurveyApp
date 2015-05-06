using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.ComponentModel;
using Windows.UI.Xaml;
using System.Runtime.CompilerServices;
using Windows.Security.Credentials;
using SurveyApp.Utils;

namespace SurveyApp
{
    /// <summary>
    /// Hlavní singleton třída starající se u udržování dat a řízení přístupů
    /// </summary>
    public class SurveySingleton : INotifyPropertyChanged
    {   
        /// <summary>
        /// privátní konstruktor jedináčka
        /// </summary>
        private SurveySingleton() { }

        /// <summary>
        /// lazy inicializace jedináčka
        /// </summary>
        public static SurveySingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SurveySingleton();
                }
                return _instance;
            }
        }

        /// <summary>
        /// instance jedináčka
        /// </summary>
        private static SurveySingleton _instance;

        /// <summary>
        /// objekt umožnující po přihlášení další interakci se SharePointem
        /// </summary>
        private ClientContext sharePointContext;

        private bool _isLoggedIn = false;
        /// <summary>
        /// slouží pro určení ,zda je uživatel přihlášený, nebo není
        /// </summary>
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            private set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }

        private string username;

        /// <summary>
        /// zabezpečené úložiště hesel
        /// </summary>
        private PasswordVault vault = new PasswordVault();

        /// <summary>
        /// Seznam definic sloupců (vyplnévaných položek) pro aktuálně zvolený dotazník
        /// </summary>
        public List<FieldInfo> CurrentFields { get; set; }

        /// <summary>
        /// Udržuje GUID aktálně zvoleného dotazníku
        /// </summary>
        public Guid CurrentList { get; set; }


        /// <summary>
        /// Obsluha přihlášení
        /// </summary>
        /// <param name="URL">adresa SharePoint stránky</param>
        /// <param name="user">uživatelské jméno</param>
        /// <param name="password">heslo</param>
        /// <param name="storePassword">má se ukládat heslo</param>
        /// <returns></returns>
        public async Task Login(string URL, string user, string password, bool storePassword)
        {
            sharePointContext = await SharePointConnector.LoginAsync(URL, user, password);
            IsLoggedIn = ((sharePointContext == null) ? false : true);
            if (IsLoggedIn)
            {
                username = user;

                PasswordCredential creds = null;

                await Task.Run(() =>
                {
                    try
                    {
                        creds = vault.FindAllByUserName(user).FirstOrDefault();
                    }
                    catch (Exception)
                    {
                    }
                });

                if (creds == null)
                {   
                    if (storePassword)
                    {
                        vault.Add(new PasswordCredential(URL, user, password));
                    }
                    else
                    {
                        vault.Add(new PasswordCredential(URL, user, " "));
                    }
                }
            }
        }

        /// <summary>
        /// zrušení všech hodnt po odhlášení
        /// </summary>
        /// <returns></returns>
        public bool Logoff()
        {
            IsLoggedIn = false;
            sharePointContext = null;
            username = null;
            CurrentList = Guid.Empty;
            CurrentFields = null;
            return IsLoggedIn;
        }

        /// <summary>
        /// vymazání přihlašovacích udajů z úložiště hesel
        /// </summary>
        public void DeleteCredentials()
        {
            var creds = vault.FindAllByUserName(username).FirstOrDefault();
            if (creds != null) { vault.Remove(creds); }
            Logoff();
        }

        /// <summary>
        /// načtení uživatelských udajů z uložistě hesel
        /// </summary>
        /// <returns></returns>
        public PasswordCredential GetSavedUser()
        {
            return vault.RetrieveAll().FirstOrDefault();
        }

        /// <summary>
        /// Získání seznamu dostupných SP listů
        /// </summary>
        /// <returns></returns>
        public async Task<List<ListInfo>> GetListsTitles()
        {
            return await SharePointConnector.LoadListTitlesAsync(sharePointContext);
        }

        /// <summary>
        /// Získání definic položek konkrétního listu
        /// </summary>
        /// <param name="listId">GUID požadovaného listu</param>
        /// <returns></returns>
        public async Task<List<FieldInfo>> GetListFields(Guid listId)
        {
            return await SharePointConnector.GetFieldsAsync(sharePointContext, listId);
        }

        /// <summary>
        /// Přidání položky na SharePoint
        /// </summary>
        /// <param name="fields">objekt s definicemi a hodnotami polí</param>
        /// <returns></returns>
        public async Task<bool> AddItem(List<FieldInfo> fields)
        {
            return await SharePointConnector.AddItem(sharePointContext, CurrentList, fields);
        }

        /// <summary>
        /// Určuje viditelnost polí, která mají být zobrazena v nepřihlášením stavu
        /// </summary>
        public Visibility VisibleWhenLoggedOff
        {
            get
            {
                if (IsLoggedIn)
                { return Visibility.Collapsed; }
                else { return Visibility.Visible; }
            }
        }

        /// <summary>
        /// Určuje viditelnost polí, která mají být zobrazena po přihlášení
        /// </summary>
        public Visibility VisibleWhenLoggedIn
        {
            get
            {
                if (IsLoggedIn)
                { return Visibility.Visible; }
                else { return Visibility.Collapsed; }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
