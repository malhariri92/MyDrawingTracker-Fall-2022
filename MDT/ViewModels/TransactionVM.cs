using MDT.Models;
using System;

namespace MDT.ViewModels
{
    public class TransactionVM
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public int TransactionTypeId { get; set; }
        public string TypeName { get; set; }
        public int? DrawId { get; set; }
        public DrawVM Draw { get; set; }


        public TransactionVM()
        {
        }

        public TransactionVM(Transaction t)
        {
            if (t != null)
            {
                TransactionId = t.TransactionId;
                UserId = t.UserId;
                UserName = t.User.UserName;
                Amount = t.Amount;
                TransactionDateTime = t.TransactionDateTime;
                TransactionTypeId = t.TransactionTypeId;
                TypeName = t.TransactionType.TypeName;
                DrawId = t.DrawId;
                if (DrawId != null)
                {
                    Draw = new DrawVM(t.Draw);
                }
            }
        }

        internal Transaction Update(Transaction item)
        {
            if (item == null)
            {
                item = new Transaction() 
                { 
                    TransactionDateTime = DateTime.Now
                };
            }

            item.UserId = UserId;
            item.TransactionTypeId = TransactionTypeId;
            item.DrawId = DrawId;
            item.Amount = Amount;
            return item;

        }
    }
}