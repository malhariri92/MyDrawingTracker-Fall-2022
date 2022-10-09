﻿using MDT.Models;
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

        [Display(Name = "Results")]
        public string Results { get; set; }

        [Display(Name ="Game")]
        [Required]
        public int DrawTypeId { get; set; }
        public string DrawTypeName { get; set; }
        List<Description> Descriptions { get; set; }
        public DrawOptionsVM Options { get; set; }

        public DrawVM()
        {
            Entries = new List<EntryVM>();
            Descriptions = new List<Description>();
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
                Options = new DrawOptionsVM(d.DrawOption);
                Entries = d.DrawEntries.Select(e => new EntryVM(e)).ToList();
            }
        }

        public void SetDescriptions(List<Description> desc)
        {
            Descriptions = desc.OrderBy(ds => ds.SortOrder).ToList();
        }

    }
}