//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ucaldas.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EVENTO
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EVENTO()
        {
            this.APOYO_ECONOMICO = new HashSet<APOYO_ECONOMICO>();
        }
    
        public string ID_EVENTO { get; set; }
        public string NOMBRE { get; set; }
        public System.DateTime FECHA_EVENTO { get; set; }
        public string TIPO { get; set; }
        public string LUGAR { get; set; }
        public decimal ESTADO { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<APOYO_ECONOMICO> APOYO_ECONOMICO { get; set; }
    }
}
