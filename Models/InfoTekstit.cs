//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KirjastoAppScrum.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class InfoTekstit
    {
        public int Infotext_ID { get; set; }
        public string InfotextContent { get; set; }
    
        public virtual Tekstit Tekstit { get; set; }
    }
}
