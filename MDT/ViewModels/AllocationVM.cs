using MDT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class AllocationVM
    {
        public int UserId { get; set; }
        public int DrawTypeId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "{0} must be greater than 0")]
        public decimal? Amount { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal AllocationBalance { get; set; }


        [Required]
        [Display(Name = "Entries Per Drawing")]
        [Range(1, 10, ErrorMessage = "{0} must be between {1} and {2}")]
        public int? EntriesPerDrawing { get; set; }

        public bool AddAllocation { get; set; }
        public bool Join { get; set; }
        public AllocationVM()
        {
            AddAllocation = true;
        }

        public AllocationVM(UserDrawTypeOption o, Balance b, Balance account) : this()
        {
            Join = o.PlayAll;
            UserId = o?.UserId ?? -1;
            DrawTypeId = o?.DrawTypeId ?? -1;
            EntriesPerDrawing = o?.MaxPlay;
            AccountBalance = account?.CurrentBalance ?? 0.0m;
            AllocationBalance = b?.CurrentBalance ?? 0.0m;
        }
    }
}