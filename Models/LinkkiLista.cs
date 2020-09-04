using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KirjastoAppScrum.Models
{
    public class LinkkiLista
    {
        public int? Refer { get; set; }
        public int? Koordinaatti { get; set; }
        public int? Id  { get; set; }
        public int? Luokka { get; set; }
        public string kieliID { get; set; }
        public string Teksti { get; set; }
    }
}