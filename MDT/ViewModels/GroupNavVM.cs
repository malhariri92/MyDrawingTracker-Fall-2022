using MDT.Models;
using MDT.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class GroupNavVM
    {
        public List<DrawTypeDTO> DrawTypes { get; set; }
        public List<DrawDTO> ActiveDraws {get; set; }
        public List<DrawDTO> InactiveDraws {get; set; }
        public List<DrawDTO> EndedDraws { get; set; }

        public GroupNavVM() 
        {
            DrawTypes = new List<DrawTypeDTO>();
            ActiveDraws = new List<DrawDTO>();
            InactiveDraws = new List<DrawDTO>();
            EndedDraws = new List<DrawDTO>();
        }

        public GroupNavVM(List<DrawType> drawTypes) :this()
        {
            foreach(DrawType dt in drawTypes)
            {
                DrawTypeDTO dtvm = new DrawTypeDTO(dt);
                ActiveDraws.AddRange(dtvm.Draws.Where(d => d.IsActive));
                InactiveDraws.AddRange(dtvm.Draws.Where(d => !d.IsActive && d.Results == null));
                EndedDraws.AddRange(dtvm.Draws.Where(d => d.Results != null));

                DrawTypes.Add(dtvm);
            }
        }
    }
}