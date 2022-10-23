using MDT.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using MDT;

namespace MDT.ViewModels
{
    public class PendingTransactionVM
    {
        public int PendingTransactionId { get; set; }
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

        public PendingTransactionVM(PendingTransaction transaction = null)
        {
            if (transaction != null)
            {
                PendingTransactionId = transaction.PendingTransactionId;
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