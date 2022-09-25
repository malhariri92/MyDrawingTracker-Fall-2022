using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class GroupUserVM
    {
        [Display(Name = "Username")]
        [Required(ErrorMessage = "{0} is required")]
        public string UserName { get; set; }

        
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Display(Name = "Access Code")]
        [Required(ErrorMessage = "{0} is required")]
        public string AccessCode { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.Password)]
        [PasswordValidation]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public GroupUserVM() { }
    }
}