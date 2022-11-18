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
        public string DrawTitle { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        public string UserName { get; set; }
        public int EntryId { get; set; }
        public int MaxCount { get; set; }

        [Display(Name = "Number of Entries")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(1,int.MaxValue, ErrorMessage ="{0} must be greater than 0")]
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
            DrawTitle = e.Draw.Title ?? $"{e.Draw.DrawType.DrawTypeName} {e.Draw.EndDateTime:yyyy-MM-dd}";
            UserId = e.UserId;
            UserName = e.User.UserName;
            EntryId = e.EntryId;
            EntryCount = 1;
            MaxCount = e.Draw.DrawOption?.MaxEntriesPerUser ?? e.Draw.DrawType.MaxEntriesPerUser;
        }
    }
}