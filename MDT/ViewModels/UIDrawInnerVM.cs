using MDT.ViewModels;
using MDT.Models.DTO;
using System.Collections.Generic;

namespace MDT.Models.ViewModels
{
    public class UIDrawInnerVM
    {
        public DrawVM drawVm { get; set; }

        public List<DrawTypeDTO> drawTypeDTOs { get; set; }

        public bool toCreate { get; set; }

        public UIDrawInnerVM() { }

        public UIDrawInnerVM(DrawVM drawVm, List<DrawTypeDTO> drawTypeDTOs, bool toCreate)
        {
            this.drawVm = drawVm;
            this.drawTypeDTOs = drawTypeDTOs;
            this.toCreate = toCreate;
        }
    }
}