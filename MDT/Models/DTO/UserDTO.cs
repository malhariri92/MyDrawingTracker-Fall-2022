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
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public int CurrentGroupId { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Create UserDTO from User entity
        /// </summary>
        /// <param name="user">User entity</param>
        public UserDTO(User user = null) {
            if (user != null)
            {
                UserId = user.UserId;
                UserName = user.UserName;
                PhoneNumber = user.PhoneNumber;
                EmailAddress = user.EmailAddress;
                CurrentGroupId = user.CurrentGroupId;
                IsVerified = user.IsVerified;
                IsActive = user.IsActive;
            }
        }

        public UserDTO(UserDetailsChangeVM user = null)
        {
            if (user != null)
            {
                UserId = user.UserId;
                UserName = user.UserName;
                PhoneNumber = user.PhoneNumber;
                EmailAddress = user.EmailAddress;
                CurrentGroupId = user.CurrentGroupId;
                IsVerified = user.IsVerified;
                IsActive = user.IsActive;
            }
        }
    }
}