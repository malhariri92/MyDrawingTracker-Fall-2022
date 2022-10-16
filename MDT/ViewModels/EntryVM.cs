using MDT.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

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

        [Display(Name = "Number of Entries")]
        [Required(ErrorMessage = "{0} is required.")]
        public int EntryCount { get; set; }

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
            EntryCount = 1;
        }
    }
}