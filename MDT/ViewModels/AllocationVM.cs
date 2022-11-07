using MDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class AllocationVM
    {
        public int UserId { get; set; }
        public int DrawTypeId { get; set; }
        public decimal Amount { get; set; }
        public int EntriesPerDrawing { get; set; }
        public bool Join { get; set; }
        public AllocationVM() 
        { 
        }

        public AllocationVM(UserDrawTypeOption o, Balance b) 
        {
            Join = o.PlayAll;
            UserId = o?.UserId ?? -1;
            DrawTypeId = o?.DrawTypeId ?? -1;
            EntriesPerDrawing = o?.MaxPlay ?? 0;
            Amount = b?.CurrentBalance ?? 0.0m;
        }
    }
}