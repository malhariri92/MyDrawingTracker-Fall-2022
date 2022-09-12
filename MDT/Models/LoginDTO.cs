using System;
using System.ComponentModel.DataAnnotations;

namespace MDT.Models
{
    /// <summary>
    /// Login in object for user account.
    /// </summary>
    public class LoginDTO
    {
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "{0} is required")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Password - case sensitive. Will be encrypted and compared with Hash in the Users table.
        /// </summary>
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Result of user login attempt
        /// </summary>
        public PasswordManager.Result LoginResult { get; set; }

        /// <summary>
        /// The User object if one exists with credentials that match those provided.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// The number of failed login attempts for the user.
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// The timestamp of the user's first failed attempt to log in.  
        /// </summary>
        public DateTime? FirstFailed { get; set; }

        /// <summary>
        /// User will be locked for 30 minutes if they have more than 3 failed login attempts within 15 minutes.
        /// </summary>
        public bool UserLocked { get; set; }


    }
}
