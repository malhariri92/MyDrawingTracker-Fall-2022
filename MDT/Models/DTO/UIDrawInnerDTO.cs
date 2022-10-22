using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models.DTO
{
    public class UIDrawInnerDTO
    {
        public DrawVM drawVm { get; set; }

        public List<DrawTypeDTO> drawTypeDTOs { get; set; }

        public bool toCreate { get; set; }

        public UIDrawInnerDTO() { }

        public UIDrawInnerDTO(DrawVM drawVm, List<DrawTypeDTO> drawTypeDTOs, bool toCreate)
        {
            this.drawVm = drawVm;
            this.drawTypeDTOs = drawTypeDTOs;
            this.toCreate = toCreate;
        }
    }
}