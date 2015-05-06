using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Runtime.Serialization.Json;
using System.IO;
using Windows.UI.Xaml.Controls;

namespace SurveyApp.Utils
{
    /// <summary>
    /// Pomocná třída obalující jinak složitý Sharepoint objekt Field o hodnoty používané v rámci apliakce
    /// </summary>
    public class FieldInfo
    {
        private int? index = null;
        private Field field = null;
        private object userValue = null;
        private List<FieldInfoItem> list = new List<FieldInfoItem>();

        /// <summary>
        /// Vrací původní SharePoint objekt
        /// </summary>
        public Field FieldObject
        {
            get { return field; }
            set { field = value; }
        }

        /// <summary>
        /// Umožńuje získat či vyplnit hodnotu vyplněnou uživatelem
        /// </summary>
        public object FieldValue
        {
            get { return userValue; }
            set { userValue = value; }
        }

        /// <summary>
        /// Umožńuje získat či vyplnit hodnotu vyplněnou uživatelem v podobě stringu
        /// </summary>
        public string FieldValueAsString
        {
            get { return userValue?.ToString(); }
            set { userValue = value; }
        }

        /// <summary>
        /// Pro položky které jsou typu Lookuup (reference na jiný objekt) uchovává možné 
        /// hodnty tohoto výběru
        /// </summary>
        public List<FieldInfoItem> LookupItems
        {
            get { return list; }
            set { list = value; }
        }

        /// <summary>
        /// Pro položky typu lookup uchovává ID vybrané položky
        /// </summary>
        public int? LookupIndex
        {
            get { return index; }
            set { index = value; }
        }

        /// <summary>
        /// Pro účely zobrazování jednotlivých polí v přehledu definuje viditelnost
        /// comboboxu s výběrem odkazovaných hodnot (pomocí bindingu)
        /// </summary>
        public Visibility LookupVisibility
        {
            get
            {
                Visibility result = Visibility.Collapsed;
                if (list?.Count > 0)
                { result = Visibility.Visible; }
                return result;
            }
        }

        /// <summary>
        /// Udržuje název každého pole pro položky připojené bindingem
        /// </summary>
        public string FieldTitle
        {
            get
            {
                return field?.Title;
            }
        }

        /// <summary>
        /// Pro potřebu urozhodování o stylu vykreslení vrací typ položky
        /// </summary>
        public string FieldType
        {
            get
            {
                return field?.TypeAsString;
            }
        }


    }

    public class FieldInfoItem
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public FieldInfoItem(int id, string title)
        {
            ID = id;
            Title = title;
        }
    }
}
