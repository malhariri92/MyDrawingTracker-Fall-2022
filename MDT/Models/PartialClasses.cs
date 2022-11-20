using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MDT.Models
{
    [MetadataType(typeof(DescriptionMetaData))]
    public partial class Description
    {
        public bool IsNew { get; set; }
        public bool IsActive { get; set; }
    }

    public class DescriptionMetaData
    {
        [MaxLength(50, ErrorMessage = "{0} cannot exceed {1} characters")]
        public string Title { get; set; }
        [Display(Name = "Text")]
        public string TextBody { get; set; }
    }

    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
    }

    public class UserMetaData
    {
        [Required]
        [Display(Name = "Name")]
        [MaxLength(50, ErrorMessage = "{0} cannot exceed {1} characters")]
        public string UserName { get; set; }
    }
}