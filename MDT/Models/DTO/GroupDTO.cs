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
        public int? ParentGroupId { get; set; }
        public string ParentGroupName { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        public bool? IsApproved { get; set; }
        public bool JoinConfirmationRequired { get; set; }

        public List<UserDTO> Admins { get; set; }
        public List<UserDTO> Users { get; set; }
        public List<GroupDTO> Subgroups { get; set; }

        /// <summary>
        /// Create a GroupDTO from a Group entity
        /// </summary>
        /// <param name="group">Group entity, must include ParentGroup, GroupUsers, Subgroups, and Subgroup GroupUsers </param>
        public GroupDTO(Group group = null)
        {
            Admins = new List<UserDTO>();
            Users = new List<UserDTO>();
            Subgroups = new List<GroupDTO>();
            if (group != null)
            {
                GroupId = group.GroupId;
                GroupName = group.GroupName;
                ParentGroupId = group.ParentGroupId;
                ParentGroupName = group.ParentGroup?.GroupName;
                IsActive = group.IsActive;
                IsPrimary = group.IsPrimary;
                IsApproved = group.IsApproved;
                JoinConfirmationRequired = group.JoinConfirmationRequired;

                foreach (GroupUser gu in group.GroupUsers)
                {
                    if (gu.IsAdmin)
                    {
                        Admins.Add(new UserDTO(gu.User));
                    }
                    else
                    {
                        Users.Add(new UserDTO(gu.User));
                    }
                }

                foreach (Group sub in group.SubGroups)
                {
                    GroupDTO sg = new GroupDTO()
                    {
                        GroupId = sub.GroupId,
                        GroupName = sub.GroupName,
                        ParentGroupId = group.GroupId,
                        ParentGroupName = group.GroupName,
                        IsActive = sub.IsActive,
                        IsPrimary = sub.IsPrimary
                    };

                    foreach(GroupUser gu in sub.GroupUsers)
                    {
                        if (gu.IsAdmin)
                        {
                            sg.Admins.Add(new UserDTO(gu.User));
                        }
                        else
                        {
                            sg.Users.Add(new UserDTO(gu.User));
                        }
                    }

                    Subgroups.Add(sg);
                }

            }
        }
    }
}