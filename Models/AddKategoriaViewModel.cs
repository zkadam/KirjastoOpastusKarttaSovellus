using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KirjastoAppScrum.Models;

namespace KirjastoAppScrum.Models
{
    public class AddKategoriaViewModel
    {
        // Kategorian lisäykseen tarvittava ViewModel
        public int KategoriaID { get; set; }
        public string SN { get; set; }
        public string TekstiFI { get; set; }
        public string TekstiSE { get; set; }
        public string TekstiEN { get; set; }
        public Nullable<int> ReferTo { get; set; }
        public int Class { get; set; }

        public string InfoTekstiFI { get; set; }
        public string InfoTekstiSE { get; set; }
        public string InfoTekstiEN { get; set; }
    }
}