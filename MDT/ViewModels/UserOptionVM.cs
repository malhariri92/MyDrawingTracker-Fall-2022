using MDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class UserOptionVM
    {
        public int UserId { get; set; }
        public int DrawTypeId { get; set; }
        public string Name { get; set; }
        public bool IsApproved { get; set; }

        public UserOptionVM()
        {

        }

        public UserOptionVM(UserDrawTypeOption o)
        {
            if (o != null)
            {
                UserId = o.UserId;
                DrawTypeId = o.DrawTypeId;
                Name = o.User.UserName;
                IsApproved = o.IsApproved;
            }
        }
    }
}