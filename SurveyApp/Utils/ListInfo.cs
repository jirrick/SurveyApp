using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveyApp.Utils
{
    /// <summary>
    /// pomocná třída uchvávající klíčové informace o SP objektu List
    /// </summary>
    public class ListInfo
    {
        public Guid ID { get; private set; }
        public string Title { get; private set; }

        /// <summary>
        /// Pro potřebu bindingu při výběru jednotlivých listů urdžuje dvojice GUID identifikátoru a názvu seznamu
        /// Uřivateli je zobrazován název seznamu, ale vnitřní zpracování probíhá na základě GUID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        public ListInfo(Guid id, string title)
        {
            this.ID = id;
            this.Title = title;
        }
    }
}
