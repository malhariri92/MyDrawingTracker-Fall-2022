using MDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class GroupNavVM
    {
        public List<DrawTypeVM> DrawTypes { get; set; }
        public List<DrawVM> ActiveDraws {get; set; }
        public List<DrawVM> InactiveDraws {get; set; }
        public List<DrawVM> EndedDraws { get; set; }

        public GroupNavVM() 
        {
            DrawTypes = new List<DrawTypeVM>();
            ActiveDraws = new List<DrawVM>();
            InactiveDraws = new List<DrawVM>();
            EndedDraws = new List<DrawVM>();
        }

        public GroupNavVM(List<DrawType> drawTypes) :this()
        {
            foreach(DrawType dt in drawTypes)
            {
                DrawTypeVM dtvm = new DrawTypeVM(dt);
                ActiveDraws.AddRange(dtvm.Draws.Where(d => d.IsActive));
                InactiveDraws.AddRange(dtvm.Draws.Where(d => !d.IsActive));
                EndedDraws.AddRange(dtvm.Draws.Where(d => d.Results != null));

                DrawTypes.Add(dtvm);
            }
        }
    }
}