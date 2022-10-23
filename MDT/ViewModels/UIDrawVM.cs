using MDT.Models.ViewModels;
using MDT.Models.DTO;
using System.Collections.Generic;

namespace MDT.ViewModels
{
    public class UIDrawVM
    {
        public DrawVM drawVm { get; set; }

        public List<DrawDTO> drawDTOs { get; set; }

        public DrawTypeVM drawTypeVM { get; set; }

        public string actionName { get; set; }

        public UIDrawInnerVM drawInnerVM { get; set; }

        public UIDrawVM() { }

        public UIDrawVM(DrawVM drawVm, List<DrawDTO> drawDTOs, DrawTypeVM drawTypeVM, string actionName, UIDrawInnerVM drawInnerVM)
        {
            this.drawVm = drawVm;
            this.drawDTOs = drawDTOs;
            this.drawTypeVM = drawTypeVM;
            this.actionName = actionName;
            this.drawInnerVM = drawInnerVM;
        }
    }
}