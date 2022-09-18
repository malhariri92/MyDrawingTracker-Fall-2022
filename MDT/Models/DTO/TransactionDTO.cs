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

        /// <summary>
        /// Create a TransactionDTO from a Transaction entity
        /// </summary>
        /// <param name="transaction">Transaction entity, must include TransactionType, User, FromLedger, and ToLedger</param>
       public TransactionDTO(Transaction transaction = null)
        {
            if (transaction !=null)
            {
                TransactionId = transaction.TransactionId;
                TransactionTypeId = transaction.TransactionTypeId;
                TransactionTypeName = transaction.TransactionType.TypeName;
                UserId = transaction.UserId;
                UserName = transaction.User.UserName;
                Amount = transaction.Amount;
                TransactionDateTime = transaction.TransactionDateTime;
                DrawId = transaction.DrawId;
                SourceLedger = transaction.SourceLedger;
                SourceLedgerName = transaction.FromLedger.LedgerName;
                DestinationLedger = transaction.DestinationLedger;
                DestinationLedgerName = transaction.ToLedger.LedgerName;
            }
        }
    }
}