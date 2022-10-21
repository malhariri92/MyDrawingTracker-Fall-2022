using MDT.ViewModels;
using MDT.Filters;
using MDT.Models;
using MDT.Models.DTO;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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


        [HttpGet]
        public ActionResult CreateDrawType(int id = 0)
        {
            DrawTypeVM vm = new DrawTypeVM();

            if (id == 0)
            {               
                vm.HasSchedule = false;
                
                return View(vm);
            }
            else
            {
                DrawType dt = db.DrawTypes.Find(id);
                vm = new DrawTypeVM(dt);
                List<Schedule> schedules = db.Schedules.Where(sc => sc.DrawTypeId == vm.DrawTypeId).ToList<Schedule>();

                if (schedules.Count > 0)
                {
                    vm.Schedule = new ScheduleVM(schedules);
                    vm.HasSchedule = true;
                }
                else
                {
                    vm.HasSchedule = false;
                    vm.Schedule = new ScheduleVM();
                }
                return View(vm);
                
            }
            
        }

        [HttpPost]
        public ActionResult CreateDrawType(DrawTypeVM vm)
        {
            if (ModelState.IsValid)
            {
                DrawType drawType = new DrawType()
                {
                    DrawTypeId = vm.DrawTypeId,
                    DrawTypeName = vm.GameName,
                    EntryCost = vm.EntryCost,
                    IsActive = vm.IsActive,
                    IsInternal = vm.IsInternal,
                    PassDrawnToNext = vm.PassDrawnToNext,
                    PassUndrawnToNext = vm.PassUndrawnToNext,
                    EntriesToDraw = vm.EntriesToDraw,
                    MaxEntriesPerUser = vm.MaxEntriesPeruser,
                    RemoveDrawnEntries = vm.RemoveDrawnEntries,
                    RemoveDrawnUsers = vm.RemoveDrawnUsers,
                    JoinConfirmationRequired = vm.JoinConfirmationRequired,
                    RefundConfirmationRequired = vm.RefundConfirmationRequired,
                    AutoDraw = vm.AutoDraw,
                    NumberOfDraws = vm.NumberOfDraws,
                    InitialUserBalance = vm.InitialUserBalance,

                };

                if (drawType.DrawTypeId == 0)
                {
                    db.Entry(drawType).State = EntityState.Added;
                    db.SaveChanges();
                    return View(new DrawTypeVM());
                }
                else
                {
                    db.Entry(drawType).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (vm.HasSchedule)
                {
                    for(int i = 0; i < vm.Schedule.Days.Count; i ++)
                    {
                        ScheduleDayVM schedule = vm.Schedule.Days[i];

                        if (!schedule.DrawTime.Equals(TimeSpan.Parse("00:00:00")))
                        {
                            Schedule sch = new Schedule()
                            {
                                DrawTypeId = vm.DrawTypeId,
                                DayOfWeek = i,
                                Time = schedule.DrawTime
                            };

                            if (db.Schedules.FirstOrDefault(s => s.DrawTypeId == sch.DrawTypeId && s.DayOfWeek == sch.DayOfWeek) != null)
                            {
                                sch = db.Schedules.FirstOrDefault(s => s.DrawTypeId == sch.DrawTypeId && s.DayOfWeek == sch.DayOfWeek);
                                sch.Time = schedule.DrawTime;
                                db.Entry(sch).State = EntityState.Modified;
                            }
                            else
                            {
                               db.Schedules.Add(sch);
                            }
                            
                        }

                    }
                    db.SaveChanges();
                    
                }
                 else
                {
                    db.Schedules.RemoveRange(db.Schedules.Where(s => s.DrawTypeId == vm.DrawTypeId));
                    db.SaveChanges();
                }

                
            }
            return View(vm);  
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
            
            /*if (draw == null)
            {
                draw = db.Draws.Find(id);
            }*/

            if (draw != null)
            {
                DrawVM vm = new DrawVM(draw);
                vm.SetDescriptions(db.Descriptions.Where(dsc => dsc.ObjectTypeId == 3 && dsc.ObjectId == vm.DrawId).ToList());

                int dtId = draw.DrawTypeId;
                DrawTypeVM dtVM = new DrawTypeVM();
                if (dtId == 0)
                {
                    dtVM.HasSchedule = false;
                }
                else
                {
                    DrawType dt = db.DrawTypes.Find(dtId);
                    dtVM = new DrawTypeVM(dt);
                    List<Schedule> schedules = db.Schedules.Where(sc => sc.DrawTypeId == vm.DrawTypeId).ToList<Schedule>();

                    if (schedules.Count > 0)
                    {
                        dtVM.Schedule = new ScheduleVM(schedules);
                        dtVM.HasSchedule = true;
                    }
                    else
                    {
                        dtVM.HasSchedule = false;
                        dtVM.Schedule = new ScheduleVM();
                    }
                }

                GroupDTO group = (GroupDTO)Session["group"];
                List<DrawDTO> drawsList = GetDraws(group.GroupId);

                List<DrawTypeDTO> dtDTO = GetDrawTypes(group.GroupId);

                Tuple<DrawVM, List<DrawTypeDTO>, bool> cedTuple = Tuple.Create(vm, dtDTO, true);

                /*Tuple<DrawVM, DrawTypeVM, List<DrawDTO>, Tuple<DrawVM, List<DrawTypeDTO>, bool>, string> tuple 
                    = Tuple.Create(vm, dtVM, drawsList, cedTuple, "viewDraw");
                return View(tuple);*/

                Tuple<DrawVM, DrawTypeVM, List<DrawDTO>, string> tuple
                    = Tuple.Create(vm, dtVM, drawsList, "viewDraw");
                return View(tuple);
                //return RedirectToAction("Index");
            }
            return View();
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