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

        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required(ErrorMessage = "{0} is required.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Results")]
        public string Results { get; set; }

        [Display(Name ="Game")]
        [Required(ErrorMessage = "{0} is required.")]
        public int DrawTypeId { get; set; }
        public string DrawTypeName { get; set; }

        public DrawVM()
        {
            Entries = new List<EntryVM>();
        }

        public DrawVM(Draw d) :this()
        {
            if (d != null)
            {
                DrawId = d.DrawId;
                StartDate = d.StartDateTime;
                EndDate = d.EndDateTime;
                Results = d.Results;
                DrawTypeId = d.DrawTypeId;
                DrawTypeName = d.DrawType.DrawTypeName;
                Entries = d.DrawEntries.Select(e => new EntryVM(e)).ToList();
            }
        }

    }
}