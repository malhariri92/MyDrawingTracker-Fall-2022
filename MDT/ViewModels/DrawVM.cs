using MDT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MDT.ViewModels
{
    public class DrawVM
    {
        public int DrawId { get; set; }
        public List<EntryVM> Entries { get; set; }

        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Results")]
        public string Results { get; set; }

        [Display(Name = "Game")]
        [Required]
        public int DrawTypeId { get; set; }
        public string DrawTypeName { get; set; }
        public List<Description> Descriptions { get; set; }

        public int MaxEntriesPerUser { get; set; }
        public int EntriesToDraw { get; set; }
        public bool RemoveDrawnEntries { get; set; }
        public bool RemoveDrawnUsers { get; set; }
        public int? NextDrawId { get; set; }
        public bool PassDrawnToNext { get; set; }
        public bool PassUndrawnToNext { get; set; }
        public bool AutoDraw { get; set; }
        public bool JoinConfirmationRequired { get; set; }
        public bool RefundConfirmationRequired { get; set; }
        public decimal EntryCost { get; set; }

        public DrawVM()
        {
            Entries = new List<EntryVM>();
        }

        public DrawVM(Draw d) : this()
        {
            if (d != null)
            {
                DrawId = d.DrawId;
                StartDate = d.StartDateTime;
                EndDate = d.EndDateTime;
                Results = d.Results;
                IsActive = d.StartDateTime != null && d.EndDateTime < DateTime.Now && Results == null;
                DrawTypeId = d.DrawTypeId;
                DrawTypeName = d.DrawType.DrawTypeName;
                EntryCost = d.DrawType.EntryCost;

                Entries = d.DrawEntries.Select(e => new EntryVM(e)).ToList();

                if (d.DrawOption != null)
                {
                    MaxEntriesPerUser = d.DrawOption.MaxEntriesPerUser;
                    EntriesToDraw = d.DrawOption.EntriesToDraw;
                    RemoveDrawnEntries = d.DrawOption.RemoveDrawnEntries;
                    RemoveDrawnUsers = d.DrawOption.RemoveDrawnUsers;
                    NextDrawId = d.DrawOption.NextDrawId;
                    PassDrawnToNext = d.DrawOption.PassDrawnToNext;
                    PassUndrawnToNext = d.DrawOption.PassUndrawnToNext;
                    AutoDraw = d.DrawOption.AutoDraw;
                    JoinConfirmationRequired = d.DrawOption.JoinConfirmationRequired;
                    RefundConfirmationRequired = d.DrawOption.RefundConfirmationRequired;
                }

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

        public void SetDefaultOptions(DrawType dt)
        {
            MaxEntriesPerUser = dt.MaxEntriesPerUser;
            EntriesToDraw = dt.EntriesToDraw;
            RemoveDrawnEntries = dt.RemoveDrawnEntries;
            RemoveDrawnUsers = dt.RemoveDrawnUsers;
            PassDrawnToNext = dt.PassDrawnToNext;
            PassUndrawnToNext = dt.PassUndrawnToNext;
            AutoDraw = dt.AutoDraw;
            JoinConfirmationRequired = dt.JoinConfirmationRequired;
            RefundConfirmationRequired = dt.RefundConfirmationRequired;
            EntryCost = dt.EntryCost;
        }
    }
}