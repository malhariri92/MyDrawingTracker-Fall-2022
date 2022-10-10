using System;
using MDT.Models;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace MDT.ViewModels
{
    public class DrawTypeVM
    {
        public int DrawTypeId { get; set; }
        [Display(Name = "Game Name")]
        [Required (ErrorMessage = "{0} is required")]
        [MaxLength (50, ErrorMessage = "Game name cannot exceed {1} characters")]
        public string GameName { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Entry cost must be greataer than $0.0")]
        [Display(Name = "Entry Cost")]
        public decimal EntryCost { get; set; }

        public List<string> Users { get; set; }

        [Display(Name = "Is active?")]
        public bool IsActive { get; set; }

        [Display(Name = "Is internal?")]
        public bool IsInternal { get; set; }

        [Display(Name = "Number of entries to draw")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value greater than 0.")]
        public int EntriesToDraw { get; set; }

        [Display(Name = "Max entries per user")]
        public int MaxEntriesPeruser { get; set; }

        [Display(Name = "Remove drawn entries?")]
        public bool RemoveDrawnEntries { get; set; }

        [Display(Name = "Remove drawn users?")]
        public bool RemoveDrawnUsers { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of draws must be greater than 1")]
        [Display(Name = "Number of draws")]
        public int NumberOfDraws { get; set; }

        [Display(Name = "Pass drawn to next?")]
        public bool PassDrawnToNext { get; set; }

        [Display(Name = "Pass undrawn to next?")]
        public bool PassUndrawnToNext { get; set; }

        [Display(Name = "Auto draw?")]
        public bool AutoDraw { get; set; }

        [Display(Name = "Join confirmation required?")]
        public bool JoinConfirmationRequired { get; set; }

        [Display(Name = "Refund confirmation required?")]
        public bool RefundConfirmationRequired { get; set; }

        [Display(Name = "Initial user balance?")]
        [Range(1, double.MaxValue, ErrorMessage = "Initial user balance must be greater than $0.0")]
        public decimal InitialUserBalance { get; set; }

        [Display(Name = "Has A Schedule?")]
        public bool HasSchedule { get; set; }

        public ScheduleVM Schedule { get; set; }

        public DrawTypeVM() { }
        public DrawTypeVM(DrawType g) 
        {
            if (g != null)
            {
                DrawTypeId = g.DrawTypeId;
                GameName = g.DrawTypeName;
                EntryCost = g.EntryCost;
                Users = g.UserDrawTypeOptions.Where(p => p.UserId != 0).OrderBy(p => p.User.UserName).Select(p => p.User.UserName).ToList();
                Schedule = new ScheduleVM(g.Schedules.ToList());
                IsActive = g.IsActive;
                IsInternal = g.IsInternal;
                PassDrawnToNext = g.PassDrawnToNext;
                PassUndrawnToNext = g.PassUndrawnToNext;
                EntriesToDraw = g.EntriesToDraw;
                MaxEntriesPeruser = g.MaxEntriesPerUser;
                RemoveDrawnEntries = g.RemoveDrawnEntries;
                RemoveDrawnUsers = g.RemoveDrawnUsers;
                JoinConfirmationRequired = g.JoinConfirmationRequired;
                RefundConfirmationRequired = g.RefundConfirmationRequired;
                AutoDraw = g.AutoDraw;
                NumberOfDraws = g.NumberOfDraws;
                InitialUserBalance = g.InitialUserBalance;
            }
        }
    }
}