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
        public bool JoinConfirmation { get; set; }
        public string AccessCode { get; }
        public List<UserVM> Admins { get; set; }
        public List<UserVM> Members { get; set; }
        public List<UserVM> PendingMembers { get; set; }
        public List<GroupInvite> Invites { get; set; }
        public Dictionary<int, List<Description>> Descriptions { get; set; }
        public List<Description> InfoDesc { get; set; }
        public string TextArea { get; set; }
        public int OwnerUserId { get; set; }

        public GroupVM()
        {
            Admins = new List<UserVM>();
            Members = new List<UserVM>();
            PendingMembers = new List<UserVM>();
            Invites = new List<GroupInvite>();
            Descriptions = new Dictionary<int, List<Description>>();
            InfoDesc = new List<Description>();
        }

        public GroupVM(Group g) : this()
        {

            if (g != null)
            {
                GroupId = g.GroupId;
                GroupName = g.GroupName;
                Admins = g.GroupUsers.Where(u => u.IsApproved && u.IsAdmin).Select(u => new UserVM(u)).ToList();
                Members = g.GroupUsers.Where(u => u.IsApproved && !u.IsAdmin).Select(u => new UserVM(u)).ToList();
                PendingMembers = g.GroupUsers.Where(u => !u.IsApproved).Select(u => new UserVM(u)).ToList();
                OwnerUserId = g.GroupUsers.Where(u => u.IsOwner).Select(u => u.UserId).FirstOrDefault();
                Invites = g.GroupInvites.ToList();
                AccessCode = g.AccessCode;
                JoinConfirmation = g.JoinConfirmationRequired;
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

            if (!Descriptions.ContainsKey(1))
            {
                Descriptions.Add(1, new List<Description>() {
                    new Description() { ObjectId =  GroupId, ObjectTypeId = 1, SortOrder = 1},
                    new Description() { ObjectId =  GroupId, ObjectTypeId = 1, SortOrder = 2},
                    new Description() { ObjectId =  GroupId, ObjectTypeId = 1, SortOrder = 3},
                });
            }

            InfoDesc = Descriptions[1];
        }
    }
}