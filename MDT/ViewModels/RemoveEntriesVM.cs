using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MDT.ViewModels
{
    public class RemoveEntriesVM : Controller
    {
        public int UserId { get; set; }
        public int DrawId { get; set; }

        [Display(Name = "Number to Remove")]
        public int RemovedEntries { get; set; }
    }
}