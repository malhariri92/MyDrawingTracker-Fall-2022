using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class SubGroupVM
    {
        [Required(ErrorMessage = "Group name is required")]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        public SubGroupVM()
        { }
    }
}