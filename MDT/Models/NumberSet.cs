//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MDT.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class NumberSet
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NumberSet()
        {
            this.DrawTypes = new HashSet<DrawType>();
        }
    
        public int NumberSetId { get; set; }
        public string TypeName { get; set; }
        public int RangeMinimum { get; set; }
        public int RangeMaximum { get; set; }
        public int DrawnCount { get; set; }
        public bool SingleSet { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DrawType> DrawTypes { get; set; }
    }
}
