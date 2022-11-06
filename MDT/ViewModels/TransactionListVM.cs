using MDT.Models;
using MDT.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class TransactionListVM
    {
        public List<TransactionDTO> Transactions { get; set; }
        public List<TransactionDTO> PendingTransactions { get; set; }

        public TransactionListVM()
        {
            Transactions = new List<TransactionDTO>();
            PendingTransactions = new List<TransactionDTO>();
        }

        public TransactionListVM(List<Transaction> t, List<PendingTransaction> pt)
        {
            Transactions = t.Select(x => new TransactionDTO(x)).ToList();
            PendingTransactions = pt.Select(x => new TransactionDTO(x)).ToList();
        }
    }
}