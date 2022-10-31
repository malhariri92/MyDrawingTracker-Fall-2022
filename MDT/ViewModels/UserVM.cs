using MDT.Models;
using System.Collections.Generic;
using System.Linq;

namespace MDT.ViewModels
{
    public class UserVM
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public List<Balance> Balances { get; set; }
        public int CurrentGroupId { get; set; }
        public List<int> AdminGroups { get; set; }
        public List<int> MemberGroups { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }



        public UserVM()
        {

        }

        public UserVM(User u)
        {
            if (u != null)
            {
                UserId = u.UserId;
                UserName = u.UserName;
                EmailAddress = u.EmailAddress;
                CurrentGroupId = u.CurrentGroupId;
                IsVerified = u.IsVerified;
                IsActive = u.IsActive;
                Balances = u.Balances.Where(x => x.UserId == u.UserId && x.Ledger.GroupId == CurrentGroupId).ToList();
                AdminGroups = u.GroupUsers.Where(g => g.IsAdmin).Select(g => g.GroupId).ToList();
                MemberGroups = u.GroupUsers.Select(g => g.GroupId).ToList();
            }
        }

        public UserVM(GroupUser gu)
        {
            if (gu != null)
            {
                UserId = gu.UserId;
                UserName = gu.User.UserName;
                EmailAddress = gu.User.EmailAddress;

            }
        }
    }
}