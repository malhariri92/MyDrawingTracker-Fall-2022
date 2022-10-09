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

            Tuple<DrawVM, List<DrawTypeDTO>, string> tuple = Tuple.Create(vm, GetDrawTypes(group.GroupId), "Create");

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
                Tuple<DrawVM, List<DrawTypeDTO>> tup = Tuple.Create(vm, GetDrawTypes(group.GroupId));
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
       
        [AdminFilter(Role = "Admin")]
        [HttpPost]
        public void Edit (string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
            GroupDTO group = (GroupDTO)Session["group"];
        }

        [AdminFilter(Role = "Admin")]
        [HttpPost]
        public ActionResult EditDraw (DrawDTO draw)
        {
            System.Diagnostics.Debug.WriteLine(draw != null);
            GroupDTO group = (GroupDTO)Session["group"];

            Tuple<DrawVM, List<DrawTypeDTO>, string> tuple = 
                Tuple.Create(
                    new DrawVM()
                    {
                       DrawId = draw.DrawId,
                       DrawTypeId = draw.DrawTypeId,
                       DrawTypeName = draw.DrawTypeName,
                       EndDate = draw.EndDateTime,
                       StartDate = draw.StartDateTime,
                    }, GetDrawTypes(group.GroupId), 
                    "Edit");
            return PartialView("CreateEditDrawForm", tuple);
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult EditList()
        {
            GroupDTO group = (GroupDTO)Session["group"];
            return View(GetDraws(group.GroupId));
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