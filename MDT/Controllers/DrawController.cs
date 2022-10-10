using MDT.Filters;
using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;

namespace MDT.Controllers
{
    public class DrawController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

       
        [AdminFilter(Role = "Admin")]
        public ActionResult CreateDraw()
        {
            GroupDTO group = (GroupDTO)Session["group"];

            Session["past"] = false;

            DrawVM vm = new DrawVM
            {
                EndDate = DateTime.Now.Date.AddDays(1)
            };

            Tuple<DrawVM, List<DrawTypeDTO>, bool> tuple = Tuple.Create(vm, GetDrawTypes(group.GroupId), true);

            return View("CreateDraw", tuple);
        }

        [AdminFilter(Role = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewDraw([Bind(Prefix = "Item1")] DrawVM vm)
        {
            GroupDTO group = (GroupDTO)Session["group"];

            Session["past"] = false;

            if (vm.EndDate < DateTime.Now.Date)
            {
                Session["past"] = true;
                Tuple<DrawVM, List<DrawTypeDTO>, bool> tup = Tuple.Create(vm, GetDrawTypes(group.GroupId), true);
                return PartialView("AddDraw", tup);
            }

            DrawType drawType = db.DrawTypes.Where(dT => dT.DrawTypeId == vm.DrawTypeId).FirstOrDefault();

            vm.DrawTypeName = drawType.DrawTypeName;

            Draw draw = new Draw()
            {
                DrawType = drawType,
                DrawTypeId = vm.DrawTypeId,
                EndDateTime = vm.EndDate,
            };

            db.Entry(draw).State = EntityState.Added;
            db.SaveChanges();

            return PartialView("DrawCreated");
        }
        


        public ActionResult EditList()
        {
            GroupDTO group = (GroupDTO)Session["group"];
            return View(GetDraws(group.GroupId));
        }

        [HttpPost]
        public ActionResult EditDraw(int DrawId)
        {
            Session["past"] = false;
            GroupDTO group = (GroupDTO)Session["group"];
            System.Diagnostics.Debug.WriteLine(DrawId);
            Draw draw = db.Draws.Where(d => d.DrawId == DrawId).FirstOrDefault();
            DrawVM vm = new DrawVM()
            {
                DrawId = draw.DrawId,
                DrawTypeId = draw.DrawTypeId,
                DrawTypeName = draw.DrawType.DrawTypeName,
                EndDate = draw.EndDateTime,
            };

            Tuple<DrawVM, List<DrawTypeDTO>, bool> tuple = Tuple.Create(vm, GetDrawTypes(group.GroupId), false);
            return View("CreateDraw", tuple);
        }

        [AdminFilter(Role = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditExistingDraw([Bind(Prefix = "Item1")] DrawVM vm)
        {
            GroupDTO group = (GroupDTO)Session["group"];

            if (vm.EndDate < DateTime.Now.Date)
            {
                Session["past"] = true;
                Tuple<DrawVM, List<DrawTypeDTO>, bool> tuple = Tuple.Create(vm, GetDrawTypes(group.GroupId), false);
                return View("CreateDraw", tuple);
            }
            Session["past"] = false;
            Draw draw = db.Draws.Where(d => d.DrawId == vm.DrawId).FirstOrDefault();
            draw.DrawTypeId = vm.DrawTypeId;
            draw.DrawType = db.DrawTypes.Where(dT => dT.DrawTypeId == vm.DrawTypeId).FirstOrDefault();
            draw.EndDateTime = vm.EndDate;

            db.Entry(draw).State = EntityState.Modified;
            db.SaveChanges();

            return PartialView("DrawEdited");
        }

        public ActionResult ViewDraw(int id)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == id && d.DrawType.GroupDrawTypes.Any(g => g.GroupId == group.GroupId)).Include(d => d.DrawType).Include(d => d.DrawOption).FirstOrDefault();

            if (draw != null)
            {
                DrawVM vm = new DrawVM(draw);
                vm.SetDescriptions(db.Descriptions.Where(dsc => dsc.ObjectTypeId == 3 && dsc.ObjectId == vm.DrawId).ToList());
                return View(vm);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role ="Admin")]
        public ActionResult DescriptionEdit(DrawVM vm)
        {
            foreach(Description desc in db.Descriptions.Where(d => d.ObjectTypeId == 3 && d.ObjectId == vm.DrawId))
            {
                db.Entry(desc).State = EntityState.Deleted;
            }
            db.SaveChanges();

            vm.Descriptions = vm.Descriptions.Where(d => d.IsActive).ToList();

            int sort = 1;
            foreach (Description d in vm.Descriptions)
            {
                d.ObjectTypeId = 3;
                d.ObjectId = vm.DrawId;
                d.SortOrder = sort;
                db.Entry(d).State = EntityState.Added;
                sort++;
            }

            db.SaveChanges();

            return RedirectToAction("ViewDraw", new { id = vm.DrawId });
        }

        public ActionResult DrawDescription(int id)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == id && d.DrawType.GroupDrawTypes.Any(g => g.GroupId == group.GroupId)).Include(d => d.DrawType).Include(d => d.DrawOption).FirstOrDefault();

            if (draw != null)
            {
                DrawVM vm = new DrawVM(draw);
                vm.SetDescriptions(db.Descriptions.Where(dsc => dsc.ObjectTypeId == 3 && dsc.ObjectId == vm.DrawId).ToList());
                return PartialView(vm);
            }
            return PartialView("Nope");
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult DrawDescriptionEdit(int id)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == id && d.DrawType.GroupDrawTypes.Any(g => g.GroupId == group.GroupId)).Include(d => d.DrawType).Include(d => d.DrawOption).FirstOrDefault();

            if (draw != null)
            {
                DrawVM vm = new DrawVM(draw);
                vm.SetDescriptions(db.Descriptions.Where(dsc => dsc.ObjectTypeId == 3 && dsc.ObjectId == vm.DrawId).ToList());
                return PartialView("DrawDescriptionEdit", vm);
            }
            return PartialView("Nope");
        }

        private List<DrawTypeDTO> GetDrawTypes(int GroupId)
        {
            List<int> ids = GetIds(GroupId);
            return db.DrawTypes
                .Where(dT => ids.Contains(dT.DrawTypeId))
                .ToList()
                .Select(dT =>
                    new DrawTypeDTO(dT)
                 ).ToList();
        }

        private List<DrawDTO> GetDraws(int GroupId)
        {
            List<int> ids = GetIds(GroupId);
            return db.Draws.Where(draw => ids.Contains(draw.DrawTypeId))
                .ToList()
                .Select(draw =>
                    new DrawDTO()
                    {
                        DrawId = draw.DrawId,
                        DrawTypeId = draw.DrawTypeId,
                        DrawTypeName = draw.DrawType.DrawTypeName,
                        StartDateTime = draw.StartDateTime,
                        EndDateTime = draw.EndDateTime,
                    }
                ).ToList();
        }

        private List<int> GetIds (int GroupId)
        {
            return db.GroupDrawTypes.Where(gDT => gDT.GroupId == GroupId).Select(gDT => gDT.DrawTypeId).ToList();
        }
    }
}