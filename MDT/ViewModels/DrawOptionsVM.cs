using MDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class DrawOptionsVM
    {
        public int DrawId { get; set; }
        public int MaxEntriesPerUser { get; set; }
        public int EntriesToDraw { get; set; }

        public bool RemoveDrawnEntries { get; set; }
        public bool RemoveDrawnUsers { get; set; }
        public int? NextDrawId { get; set; }
        public bool PassDrawnToNext { get; set; }

        public DrawOptionsVM() { }
        public DrawOptionsVM(DrawOption o)
        {
            if (o != null)
            {
                DrawId = o.DrawId;
                MaxEntriesPerUser = o.MaxEntriesPerUser;
                EntriesToDraw = o.EntriesToDraw;
                RemoveDrawnEntries = o.RemoveDrawnEntries;
                RemoveDrawnUsers = o.RemoveDrawnUsers;
                NextDrawId = o.NextDrawId;
                PassDrawnToNext = o.PassDrawnToNext;
            }
        }
    }
}