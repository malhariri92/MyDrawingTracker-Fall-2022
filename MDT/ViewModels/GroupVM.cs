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
        public List<UserVM> Admins { get; set; }
        public List<UserVM> Members { get; set; }
        public Dictionary<int, List<Description>> Descriptions { get; set; }
        public string TextArea { get; set; }
        public int OwnerUserId { get; set; }

        public GroupVM()
        {
            Admins = new List<UserVM>();
            Members = new List<UserVM>();
            Descriptions = new Dictionary<int, List<Description>>();
        }

        public GroupVM(Group g) : this()
        {

            if (g != null)
            {
                GroupId = g.GroupId;
                GroupName = g.GroupName;
                Admins = g.GroupUsers.Where(u => u.IsAdmin).Select(u => new UserVM(u)).ToList();
                Members = g.GroupUsers.Where(u => !u.IsAdmin).Select(u => new UserVM(u)).ToList();
                OwnerUserId = g.GroupUsers.Where(u => u.IsOwner).Select(u => u.UserId).FirstOrDefault();
            }
        }

        public void SetDescriptions(List<Description> desc)
        {
            foreach (Description d in desc.OrderBy(ds => ds.ObjectTypeId).ThenBy(ds => ds.SortOrder))
            {
                if (!Descriptions.ContainsKey(d.ObjectTypeId))
                {
                    Descriptions.Add(d.ObjectTypeId, new List<Description>());
                }

                Descriptions[d.ObjectTypeId].Add(d);
            }
        }
    }
}