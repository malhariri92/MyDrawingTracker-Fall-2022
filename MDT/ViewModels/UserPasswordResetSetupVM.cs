using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class UserPasswordResetSetupVM
    {
        public int UserId { get; set; }
       
        [Display(Name = "Username")]
        [Required(ErrorMessage = "{0} is required")]
        public string UserName { get; set; }

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "{0} is required")]
        [EmailAddress]
        public string UserEmail { get; set; }

        public string UserKey { get; set; }
        public int AccountType { get; set; }

        public string Message { get; set; }
        public bool Success { get; set; }
        public bool Error { get; set; }
        public bool IsChangeRequest { get; set; }
    }
}