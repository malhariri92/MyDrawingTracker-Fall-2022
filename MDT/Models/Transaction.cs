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
    
    public partial class Transaction
    {
        public int TransactionId { get; set; }
        public int TransactionTypeId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime TransactionDateTime { get; set; }
        public Nullable<int> DrawId { get; set; }
        public int SourceLedger { get; set; }
        public int DestinationLedger { get; set; }
    
        public virtual Draw Draw { get; set; }
        public virtual Ledger Ledger { get; set; }
        public virtual Ledger Ledger1 { get; set; }
        public virtual TransactionType TransactionType { get; set; }
        public virtual User User { get; set; }
    }
}
