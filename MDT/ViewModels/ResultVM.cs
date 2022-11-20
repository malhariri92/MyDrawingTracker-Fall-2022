using MDT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class ResultVM
    {
        public int EntryId { get; set; }
        public string EntryCode { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Sequence { get; set; }

        public ResultVM()
        {
        }
        public ResultVM(DrawResult r) 
        {
            EntryId = r.EntryId;
            EntryCode = r.DrawEntry.EntryCode;
            UserId = r.DrawEntry.UserId;
            UserName = r.DrawEntry.User.UserName;
            Sequence = r.DrawCount;
        }

    }
}