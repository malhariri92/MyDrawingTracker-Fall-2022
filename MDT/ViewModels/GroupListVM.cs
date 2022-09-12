using MDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class GroupListItemVM
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<GroupListItemVM> Subgroups { get; set; }

        public GroupListItemVM()
        {

        }

        public GroupListItemVM(int userId)
        {
            using(var db = new DbEntities())
            {
                db.GroupUsers.Where(g => g.UserId == userId && g.Group.IsActive && g.Group.IsPrimary);
            }
        }

        public GroupListItemVM(DbEntities db, int userId)
        {

        }
    }
}