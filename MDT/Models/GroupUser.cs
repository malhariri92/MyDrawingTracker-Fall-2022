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
    
    public partial class GroupUser
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public bool IsAdmin { get; set; }
    
        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
    }
}
