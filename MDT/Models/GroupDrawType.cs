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
    
    public partial class GroupDrawType
    {
        public int GroupId { get; set; }
        public int DrawTypeId { get; set; }
        public int LedgerId { get; set; }
        public bool IsActive { get; set; }
    
        public virtual DrawType DrawType { get; set; }
        public virtual Ledger Ledger { get; set; }
        public virtual Group Group { get; set; }
    }
}
