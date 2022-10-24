using MDT.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using MDT;

namespace MDT.ViewModels
{
    public class TransactionVM
    {
        public int TransactionId { get; set; }

        [Display(Name = "User")]
        [Required(ErrorMessage = "{0} is required")]
        public int UserId { get; set; }
        public string UserName { get; set; }

        [Display(Name = "Amount")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "{0} must be greater than $0.00")]
        public decimal Amount { get; set; }
        public DateTime TransactionDateTime { get; set; }

        [Display(Name = "Transaction Type")]
        [Required(ErrorMessage = "{0} is required")]
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