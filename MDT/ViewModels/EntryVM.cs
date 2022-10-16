using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using MDT.Models;

namespace MDT.ViewModels
{
    public class EntryVM
    {
        public int DrawId { get; set; }
        public DateTime DrawDate { get; set; }
        public int DrawTypeId { get; set; }
        public string GameName { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        public string UserName { get; set; }
        public int EntryId { get; set; }

        [Display(Name = "Number of Entries")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(1,100, ErrorMessage ="{0} must be between {1} and {2}")]
        public int EntryCount { get; set; }
        
        public string Message { get; set; }
        public bool Success { get; set; }
        public bool Error { get; set; }

        public EntryVM()
        { }
        
        public EntryVM(DrawEntry e)
        {
            DrawId = e.DrawId;
            DrawDate = e.Draw.EndDateTime;
            DrawTypeId = e.Draw.DrawTypeId;
            GameName = e.Draw.DrawType.DrawTypeName;
            UserId = e.UserId;
            UserName = e.User.UserName;
            EntryId = e.EntryId;
            EntryCount = 1;
        }
    }
}