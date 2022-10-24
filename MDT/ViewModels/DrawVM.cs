using MDT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace MDT.ViewModels
{
    public class DrawVM
    {
        public int DrawId { get; set; }
        public List<EntryVM> Entries { get; set; }

        public string Title { get; set; }
        public string VirtualTitle { get; set; }
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required(ErrorMessage = "{0} is required.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Results")]
        public string Results { get; set; }

        [Display(Name = "Game")]
        [Required(ErrorMessage = "{0} is required.")]
        public int DrawTypeId { get; set; }
        public string DrawTypeName { get; set; }
        public List<Description> Descriptions { get; set; }
        public DrawOptionsVM Options { get; set; }

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
            Descriptions = new List<Description>();
        }

        /// <summary>
        /// Create DrawVM from Draw.
        /// </summary>
        /// <param name="d">Draw entity. Must include DrawType and DrawOption</param>
        public DrawVM(Draw d) : this()
        {
            if (d != null)
            {
                DrawId = d.DrawId;
                Title = d.Title;
                VirtualTitle = $"{d.DrawType.DrawTypeName} {d.EndDateTime:yyyy-MM-dd}";
                StartDate = d.StartDateTime;
                EndDate = d.EndDateTime;
                Results = d.Results;
                IsActive = d.StartDateTime != null && d.EndDateTime > DateTime.Now && Results == null;
                DrawTypeId = d.DrawTypeId;
                DrawTypeName = d.DrawType.DrawTypeName;

                EntryCost = d.DrawType.EntryCost;

                Entries = d.DrawEntries.Select(e => new EntryVM(e)).ToList();

                if (d.DrawOption == null)
                {
                    MaxEntriesPerUser = d.DrawType.MaxEntriesPerUser;
                    EntriesToDraw = d.DrawType.EntriesToDraw;
                    RemoveDrawnEntries = d.DrawType.RemoveDrawnEntries;
                    RemoveDrawnUsers = d.DrawType.RemoveDrawnUsers;
                    PassDrawnToNext = d.DrawType.PassDrawnToNext;
                    PassUndrawnToNext = d.DrawType.PassUndrawnToNext;
                    AutoDraw = d.DrawType.AutoDraw;
                    JoinConfirmationRequired = d.DrawType.JoinConfirmationRequired;
                    RefundConfirmationRequired = d.DrawType.RefundConfirmationRequired;
                    EntryCost = d.DrawType.EntryCost;
                }
                else
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

        public DrawVM(DrawType d) : this()
        {

            DrawTypeId = d.DrawTypeId;
            DrawTypeName = d.DrawTypeName;

            EntryCost = d.EntryCost;

            MaxEntriesPerUser = d.MaxEntriesPerUser;
            EntriesToDraw = d.EntriesToDraw;
            RemoveDrawnEntries = d.RemoveDrawnEntries;
            RemoveDrawnUsers = d.RemoveDrawnUsers;
            PassDrawnToNext = d.PassDrawnToNext;
            PassUndrawnToNext = d.PassUndrawnToNext;
            AutoDraw = d.AutoDraw;
            JoinConfirmationRequired = d.JoinConfirmationRequired;
            RefundConfirmationRequired = d.RefundConfirmationRequired;
            EntryCost = d.EntryCost;
        }


        public void SetDescriptions(List<Description> desc)
        {
            Descriptions = desc;
            foreach (Description d in Descriptions)
            {
                d.IsActive = true;
            }
        }
    }
}