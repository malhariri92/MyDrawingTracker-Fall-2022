using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class JoinVM
    {
        [Display(Name = "Access Code")]
        [Required(ErrorMessage = "{0} is required")]
        public string AccessCode { get; set; }
    }
}