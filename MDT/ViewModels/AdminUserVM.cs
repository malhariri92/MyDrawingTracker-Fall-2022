using System;
using System.ComponentModel.DataAnnotations;

namespace MDT.ViewModels
{
    public class AdminUserVM
    {

        [Display(Name = "Display Name")]
        [Required(ErrorMessage = "{0} is required")]
        [MaxLength(50, ErrorMessage = "{0} cannot exceed {1} characters")]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email address is required")]
        public string EmailAddress { get; set; }

        [Display(Name = "Group Name")]
        [Required(ErrorMessage = "Group name is required")]
        public string GroupName { get; set; }

        [Required]
        public string Reason { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [PasswordValidation]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPass { get; set; }

    }
}