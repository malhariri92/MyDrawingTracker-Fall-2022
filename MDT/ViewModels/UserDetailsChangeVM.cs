using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MDT.Models.DTO;

namespace MDT.ViewModels
{
	public class UserDetailsChangeVM
	{
		[Display(Name = "Display Name")]
		[Required(ErrorMessage = "{0} is required")]
		[DataType(DataType.Text)]
		[MaxLength(50, ErrorMessage = "{0} cannot exceed {1} characters")]
		public string UserName { get; set; }

		public string EmailAddress { get; set; }

		//[Display(Name = "Phone Number")]
		//[Required(ErrorMessage = "{0} is required")]
		//[DataType(DataType.PhoneNumber)]
		//[PhoneNumberValidation]
		//public string PhoneNumber { get; set; }

        [Display(Name = "Current Group ID")]
        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.Text)]
        public int CurrentGroupId { get; set; }

        public int UserId { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }

        public string Message { get; set; }
		public bool Success { get; set; }
		public bool Error { get; set; }
		//public bool IsChangeRequest { get; set; }

		public UserDetailsChangeVM()
		{

		}
		public UserDetailsChangeVM (UserDTO user = null) {
			if (user != null)
			{

				UserId = user.UserId;
				UserName = user.UserName;
				EmailAddress = user.EmailAddress;
				//PhoneNumber = user.PhoneNumber;
				CurrentGroupId = user.CurrentGroupId;
				IsVerified = user.IsVerified;
				IsActive = user.IsActive;
			}
		}
	}
}