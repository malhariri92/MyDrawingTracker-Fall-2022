using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models.DTO
{
    public class TransactionDTO
    {
        public int TransactionId { get; set; }
        public int TransactionTypeId { get; set; }
        public string TransactionTypeName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public int? DrawId { get; set; }
        public int SourceLedger { get; set; }
        public string SourceLedgerName { get; set; }
        public int DestinationLedger { get; set; }
        public string DestinationLedgerName { get; set; }
        public bool IsPending { get; set; }

        public TransactionDTO()
        {

        }

        /// <summary>
        /// Create a TransactionDTO from a Transaction entity
        /// </summary>
        /// <param name="t">Transaction entity, must include TransactionType, User, FromLedger, and ToLedger</param>
       public TransactionDTO(Transaction t)
        {
            if (t !=null)
            {
                TransactionId = t.TransactionId;
                TransactionTypeId = t.TransactionTypeId;
                TransactionTypeName = t.TransactionType.TypeName;
                UserId = t.UserId;
                UserName = t.User.UserName;
                Amount = t.Amount;
                TransactionDateTime = t.TransactionDateTime;
                DrawId = t.DrawId;
                SourceLedger = t.SourceLedger;
                SourceLedgerName = t.FromLedger.LedgerName;
                DestinationLedger = t.DestinationLedger;
                DestinationLedgerName = t.ToLedger.LedgerName;
                IsPending = false;
            }
        }

        /// <summary>
        /// Create a TransactionDTO from a PendingTransaction entity
        /// </summary>
        /// <param name="pt">PendingTransaction entity, must include TransactionType, User, FromLedger, and ToLedger</param>
        public TransactionDTO(PendingTransaction pt)
        {
            if (pt != null)
            {
                TransactionId = pt.PendingTransactionId;
                TransactionTypeId = pt.TransactionTypeId;
                TransactionTypeName = pt.TransactionType.TypeName;
                UserId = pt.UserId;
                UserName = pt.User.UserName;
                Amount = pt.Amount;
                TransactionDateTime = pt.TransactionDateTime;
                DrawId = pt.DrawId;
                SourceLedger = pt.SourceLedger;
                SourceLedgerName = pt.FromLedger.LedgerName;
                DestinationLedger = pt.DestinationLedger;
                DestinationLedgerName = pt.ToLedger.LedgerName;
                IsPending = true;
            }
        }
    }
}