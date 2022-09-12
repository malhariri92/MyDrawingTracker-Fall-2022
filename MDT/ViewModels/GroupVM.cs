using MDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class GroupVM
    {

        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<GroupVM> SubGroups { get; set; }
        public List<UserVM> Users { get; set; }
        List<int> GroupMembers { get; set; }
        List<int> GroupAdmins { get; set; }

        public GroupVM() { }

        public GroupVM(Group g)
        {
            if (g != null)
            {
                GroupId = g.GroupId;
                GroupName = g.GroupName;
                GroupMembers = g.GroupUsers.Where(u => u.IsAdmin).Select(u => u.UserId).ToList();
                GroupAdmins = g.GroupUsers.Select(u => u.UserId).ToList();
               // SubGroups = g.SubGroups?.Select(sg => new GroupVM(sg)).ToList();
               // Users = g.GroupUsers.Select(gp => new UserVM(gp)).ToList();
            }
        }
    }
}