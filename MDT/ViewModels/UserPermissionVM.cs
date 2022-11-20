using MDT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MDT.ViewModels
{
    public class UserPermissionVM
    {
        public UserPermissionVM()
        {

        }
        public int UserId { get; set; }
        public string UserName { get; set; }

        [Display(Name = "Can Manage Users?")]
        public bool CanManageUsers { get; set; }

        [Display(Name = "Can Manage Draw Types?")]
        public bool CanManageDrawTypes { get; set; }

        [Display(Name = "Can Manage Drawings?")]
        public bool CanManageDrawings { get; set; }

        [Display(Name = "Can Manage Transactions?")]
        public bool CanManageTransactions { get; set; }

        public UserPermissionVM(GroupUser user)
        {
            UserId = user.UserId;
            CanManageUsers = user.CanManageUsers;
            CanManageDrawTypes = user.CanManageDrawTypes;
            CanManageDrawings = user.CanManageDrawings;
            CanManageTransactions = user.CanManageTransactions;
        }
    }
}