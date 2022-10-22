using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models.DTO
{
    public class UIDrawDTO
    {
        public DrawVM drawVm { get; set; }

        public List<DrawDTO> drawDTOs { get; set; }

        public DrawTypeVM drawTypeVM { get; set; }

        public string actionName { get; set; }

        public UIDrawInnerDTO drawInnerDTO { get; set; }

        public UIDrawDTO() { }

        public UIDrawDTO(DrawVM drawVm, List<DrawDTO> drawDTOs, DrawTypeVM drawTypeVM, string actionName, UIDrawInnerDTO drawInnerDTO)
        {
            this.drawVm = drawVm;
            this.drawDTOs = drawDTOs;
            this.drawTypeVM = drawTypeVM;
            this.actionName = actionName;
            this.drawInnerDTO = drawInnerDTO;
        }
    }
}