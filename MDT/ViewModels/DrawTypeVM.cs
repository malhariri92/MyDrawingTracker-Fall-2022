using System;
using MDT.Models;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MDT.ViewModels
{
    public class DrawTypeVM
    {
        public int DrawTypeId { get; set; }

        [Display(Name = "Type Name")]
        [Required(ErrorMessage = "{0} is required")]
        [MaxLength(50, ErrorMessage = "{0} cannot exceed {1} characters")]
        public string TypeName { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Entry cost must be greataer than 0")]
        [Display(Name = "Entry Cost")]
        public decimal EntryCost { get; set; }

        public List<string> Users { get; set; }

        [Display(Name = "Is active?")]
        public bool IsActive { get; set; }

        [Display(Name = "Is internal?")]
        public bool IsInternal { get; set; }

        [Display(Name = "Entries to draw")]
        public int EntriesToDraw { get; set; }

        [Display(Name = "Max entries per user")]
        [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative")]
        public int MaxEntriesPerUser { get; set; }

        [DefaultValue(1)]
        [Range(1, int.MaxValue, ErrorMessage = "{0} must be greater than 0")]
        [Display(Name = "Number of draws")]
        public int NumberOfDraws { get; set; }

        [Display(Name = "Remove drawn entries?")]
        public bool RemoveDrawnEntries { get; set; }

        [Display(Name = "Remove drawn users?")]
        public bool RemoveDrawnUsers { get; set; }

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

        [Display(Name = "Isolate balance?")]
        public bool IsolateBalance { get; set; }

        [Display(Name = "Allow allocation?")]
        public bool AllowAllocation { get; set; }

        [Display(Name = "Initial user balance?")]
        [Range(0, double.MaxValue, ErrorMessage = "{0} cannot be negative")]
        public decimal InitialUserBalance { get; set; }

        [Display(Name = "Recurring?")]
        public bool HasSchedule { get; set; }

        public List<Description> Descriptions { get; set; }

        public ScheduleVM Schedule { get; set; }

        public List<DrawVM> Draws { get; set; }

        public List<UserOptionVM> UserOptions { get; set; }

        public DrawTypeVM()
        {
            Schedule = new ScheduleVM();
            NumberOfDraws = 1;
            Draws = new List<DrawVM>();
            Descriptions = new List<Description>();
            UserOptions = new List<UserOptionVM>();
        }

        /// <summary>
        /// Create DrawTypeVM from DrawType.
        /// </summary>
        /// <param name="dt">DrawType entity. Must include UserDrawTypeOptions, UserDrawTypeOptions Users, Schedules, Draws, and Draw Options</param>
        public DrawTypeVM(DrawType dt) : this()
        {
            if (dt != null)
            {
                DrawTypeId = dt.DrawTypeId;
                TypeName = dt.DrawTypeName;
                EntryCost = dt.EntryCost;
                Users = dt.UserDrawTypeOptions.Where(p => p.UserId != 0).OrderBy(p => p.User.UserName).Select(p => p.User.UserName).ToList();
                HasSchedule = dt.Schedules.Any();
                Schedule = new ScheduleVM(dt.Schedules.ToList());
                IsActive = dt.IsActive;
                IsInternal = dt.IsInternal;
                PassDrawnToNext = dt.PassDrawnToNext;
                PassUndrawnToNext = dt.PassUndrawnToNext;
                EntriesToDraw = dt.EntriesToDraw;
                MaxEntriesPerUser = dt.MaxEntriesPerUser;
                RemoveDrawnEntries = dt.RemoveDrawnEntries;
                RemoveDrawnUsers = dt.RemoveDrawnUsers;
                JoinConfirmationRequired = dt.JoinConfirmationRequired;
                RefundConfirmationRequired = dt.RefundConfirmationRequired;
                AutoDraw = dt.AutoDraw;
                NumberOfDraws = dt.NumberOfDraws;
                InitialUserBalance = dt.InitialUserBalance;
                IsolateBalance = dt.IsolateBalance;
                AllowAllocation = dt.AllowAllocation;
                Draws = dt.Draws.Select(d => new DrawVM(d)).ToList();
            }
        }

        public void SetDescriptions(List<Description> desc)
        {
            Descriptions = desc;
            foreach (Description d in Descriptions)
            {
                d.IsActive = true;
            }
        }

        public void SetUserOptions(List<UserDrawTypeOption> opts)
        {
            UserOptions = opts.Select(o => new UserOptionVM(o)).ToList(); 
        }
    }
}
