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
    
    public partial class DrawType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DrawType()
        {
            this.Draws = new HashSet<Draw>();
            this.Schedules = new HashSet<Schedule>();
            this.UserDrawTypeOptions = new HashSet<UserDrawTypeOption>();
            this.NumberSets = new HashSet<NumberSet>();
        }
    
        public int DrawTypeId { get; set; }
        public string DrawTypeName { get; set; }
        public int GroupId { get; set; }
        public int LedgerId { get; set; }
        public decimal EntryCost { get; set; }
        public bool IsActive { get; set; }
        public bool IsInternal { get; set; }
        public int EntriesToDraw { get; set; }
        public int MaxEntriesPerUser { get; set; }
        public bool RemoveDrawnEntries { get; set; }
        public bool RemoveDrawnUsers { get; set; }
        public int NumberOfDraws { get; set; }
        public bool PassDrawnToNext { get; set; }
        public bool PassUndrawnToNext { get; set; }
        public bool AutoDraw { get; set; }
        public bool JoinConfirmationRequired { get; set; }
        public bool RefundConfirmationRequired { get; set; }
        public decimal InitialUserBalance { get; set; }
        public bool IsolateBalance { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Draw> Draws { get; set; }
        public virtual Group Group { get; set; }
        public virtual Ledger Ledger { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schedule> Schedules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserDrawTypeOption> UserDrawTypeOptions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NumberSet> NumberSets { get; set; }
    }
}
