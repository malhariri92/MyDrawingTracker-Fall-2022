using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models.DTO
{
    public class GroupDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string AccessCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        public bool? IsApproved { get; set; }
        public bool JoinConfirmationRequired { get; set; }
        public int AccountBalanceLedgerId { get; set; }
        public List<UserDTO> Admins { get; set; }
        public List<UserDTO> Users { get; set; }
        public int OwnerId { get; set; }
        
        /// <summary>
        /// Create a GroupDTO from a Group entity
        /// </summary>
        /// <param name="group">Group entity, must include GroupUsers </param>
        public GroupDTO(Group group = null)
        {
            Admins = new List<UserDTO>();
            Users = new List<UserDTO>();
            if (group != null)
            {
                GroupId = group.GroupId;
                GroupName = group.GroupName;
                IsActive = group.IsActive;
                IsApproved = group.IsApproved;
                JoinConfirmationRequired = group.JoinConfirmationRequired;
                AccessCode = group.AccessCode;
                AccountBalanceLedgerId = group.LedgerId ?? 2;
                OwnerId = group.GroupUsers.ToList().Find(g => g.IsOwner)?.UserId ?? -1;

                foreach (GroupUser gu in group.GroupUsers)
                {
                    if (gu.IsAdmin)
                    {
                        Admins.Add(new UserDTO(gu));
                    }
                    else
                    {
                        Users.Add(new UserDTO(gu));
                    }
                }

            }
        }
    }
}