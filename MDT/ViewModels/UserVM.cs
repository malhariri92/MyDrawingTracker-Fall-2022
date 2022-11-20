using MDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDT.ViewModels
{
    public class UserVM
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public List<Balance> Balances { get; set; }
        public int CurrentGroupId { get; set; }
        public Dictionary<int,string> AdminGroups { get; set; }
        public Dictionary<int, string> MemberGroups { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public bool IsOwner { get; set; }



        public UserVM()
        {
            Balances = new List<Balance>();
            AdminGroups = new Dictionary<int, string>();
            MemberGroups = new Dictionary<int, string>();

        }

        public UserVM(User u, int groupId)
        {
            if (u != null)
            {
                UserId = u.UserId;
                UserName = u.UserName;
                EmailAddress = u.EmailAddress;
                CurrentGroupId = u.CurrentGroupId;
                IsVerified = u.IsVerified;
                IsActive = u.IsActive;
                Balances = u.Balances.Where(x => x.Ledger.GroupId == groupId).ToList();
                AdminGroups = u.GroupUsers.Where(g => g.IsAdmin).ToDictionary(g => g.GroupId, g=> g.Group.GroupName);
                MemberGroups = u.GroupUsers.Where(g => !g.IsAdmin).ToDictionary(g => g.GroupId, g => g.Group.GroupName);
                IsOwner = u.GroupUsers.Where(x => x.GroupId == groupId).FirstOrDefault().IsOwner;
            }
        }

        public UserVM(GroupUser gu)
        {
            if (gu != null)
            {
                UserId = gu.UserId;
                UserName = gu.User.UserName;
                EmailAddress = gu.User.EmailAddress;
                IsOwner = gu.IsOwner;
            }
        }
    }
}