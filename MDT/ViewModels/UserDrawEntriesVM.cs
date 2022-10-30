using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MDT.ViewModels
{
    public class UserDrawEntriesVM 
    {
        public int DrawId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int NumEntries { get; set; }

        public UserDrawEntriesVM (int DrawId, int UserId, string UserName, int NumEntries)
        {
            this.DrawId = DrawId;
            this.UserId = UserId;
            this.UserName = UserName;
            this.NumEntries = NumEntries;
        }
    }
}