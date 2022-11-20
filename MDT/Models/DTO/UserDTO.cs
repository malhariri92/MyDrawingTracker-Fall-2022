using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public int CurrentGroupId { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public bool IsOwner { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanManageDrawTypes { get; set; }
        public bool CanManageDrawings { get; set; }
        public bool CanManageTransactions { get; set; }

        /// <summary>
        /// Create UserDTO from User entity
        /// </summary>
        /// <param name="user">User entity</param>
        //public UserDTO(User user = null) {
        //    if (user != null)
        //    {
        //        UserId = user.UserId;
        //        UserName = user.UserName;
        //        EmailAddress = user.EmailAddress;
        //        CurrentGroupId = user.CurrentGroupId;
        //        IsVerified = user.IsVerified;
        //        IsActive = user.IsActive;
        //    }
        //}

        /// <summary>
        /// Create UserDTO from GroupUser entity
        /// </summary>
        /// <param name="gu">GroupUser entity must include User</param>
        public UserDTO(GroupUser gu = null)
        {
            if (gu != null)
            {
                UserId = gu.UserId;
                UserName = gu.User.UserName;
                EmailAddress = gu.User.EmailAddress;
                CurrentGroupId = gu.User.CurrentGroupId;
                IsVerified = gu.User.IsVerified;
                IsActive = gu.User.IsActive;
                IsApproved = gu.IsApproved;
                IsOwner = gu.IsOwner;
                CanManageUsers = gu.CanManageUsers;
                CanManageDrawTypes = gu.CanManageDrawTypes;
                CanManageDrawings = gu.CanManageDrawings;
                CanManageTransactions = gu.CanManageTransactions;

            }
        }
    }
}