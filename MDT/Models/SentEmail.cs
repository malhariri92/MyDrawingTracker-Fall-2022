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
    
    public partial class SentEmail
    {
        public int MessageId { get; set; }
        public int TemplateId { get; set; }
        public string Recipients { get; set; }
        public string VariablesJSON { get; set; }
        public Nullable<System.DateTime> SentOn { get; set; }
    
        public virtual EmailTemplate EmailTemplate { get; set; }
    }
}
