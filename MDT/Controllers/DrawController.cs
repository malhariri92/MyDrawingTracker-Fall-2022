using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace MDT.Controllers
{
    public class DrawController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddDraw()
        {
            GroupDTO group = (GroupDTO)Session["group"];
            UserDTO user = (UserDTO)Session["user"];

            Session["past"] = false;
     
            DrawVM vm = new DrawVM();

            List<DrawType> test = db.DrawTypes.ToList();

            foreach (DrawType dT in test)
            {
                vm.DrawTypes.Add(dT.DrawTypeId.ToString(), dT);                
            }
            
            vm.EndDate = DateTime.Now;

            return View(vm);
        }

        public ActionResult AddNewDraw(DrawVM vm)
        {
            Session["past"] = false;

            if (vm.EndDate < DateTime.Now)
            {
                Session["past"] = true;
                return PartialView("AddDraw", vm);
            }
            return View();
        }
    }
}