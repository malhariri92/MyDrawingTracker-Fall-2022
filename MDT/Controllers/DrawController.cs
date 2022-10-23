using MDT.ViewModels;
using MDT.Filters;
using MDT.Models;
using MDT.Models.DTO;
using MDT.Models.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace MDT.Controllers
{
    public class DrawController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult EditDrawType(int id = 0)
        {
            DrawTypeVM vm = GetDrawTypeVM(id);
            return View(vm);

        }

        [HttpPost]
        public ActionResult EditDrawType(DrawTypeVM vm)
        {
            if (ModelState.IsValid)
            {
                DrawType drawType = new DrawType()
                {
                    DrawTypeId = vm.DrawTypeId,
                    DrawTypeName = vm.TypeName,
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
                    Ledger gl = new Ledger()
                    {
                        GroupId = group.GroupId,
                        LedgerName = drawType.DrawTypeName,
                        Balance = 0.0m
                    };

                    db.Entry(gl).State = EntityState.Added;
                    db.SaveChanges();

                    GroupDrawType gdt = new GroupDrawType()
                    {
                        DrawTypeId = drawType.DrawTypeId,
                        IsActive = true,
                        GroupId = group.GroupId,
                        LedgerId = gl.LedgerId
                    };

                    db.Entry(gdt).State = EntityState.Added;
                    db.SaveChanges();
                    return View(new DrawTypeVM(drawType));
                }
                else
                {
                    db.Entry(drawType).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (vm.HasSchedule)
                {
                    for (int i = 0; i < vm.Schedule.Days.Count; i++)
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
 
        public ActionResult EditDraw(int id = 0)
        {
            DrawVM vm = GetDrawVM(id);
            if(id != vm.DrawId)
            {
                TempData["Error"] = "Draw not found";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.DrawTypes = GetDdl(db.DrawTypes);
            return View(vm);
        }

        [HttpPost]
        [AdminFilter(Role ="Admin")]
        public ActionResult EditDraw(DrawVM vm)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == vm.DrawId)
                              .Include(d => d.DrawType)
                              .Include(d => d.DrawType.GroupDrawTypes)
                              .Include(d => d.DrawOption)
                              .FirstOrDefault();

            if (vm.DrawId != 0 || draw == null || !GetDdl(db.Draws).Any(d => d.val == draw.DrawId))
            {
                TempData["Error"] = "Draw not found";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if(!ModelState.IsValid)
                {
                    ViewBag.DrawTypes = GetDdl(db.DrawTypes);
                    return View(vm);
                }

                draw = new Draw() 
                {
                    DrawTypeId = vm.DrawTypeId
                };
            }

            draw.EndDateTime = vm.EndDate;
            
            db.Entry(draw).State = draw.DrawId == 0 ? EntityState.Added : EntityState.Modified;
            db.SaveChanges();

            vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectTypeId == 3 && d.ObjectId == draw.DrawId).ToList());

            return View("ViewDraw", vm);
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
                Session["past"] = false;
                vm.EndDate = DateTime.Now.Date.AddDays(1);
                List<DrawDTO> drawsList = GetDraws(group.GroupId);

                List<DrawTypeDTO> dtDTOList = GetDrawTypes(group.GroupId);

                UIDrawInnerVM uIDrawInnerVM = new UIDrawInnerVM(vm, dtDTOList, false);

                UIDrawVM uidraw = new UIDrawVM(vm, drawsList, dtVM, "viewDraw", uIDrawInnerVM);
                return View(uidraw);
                //return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin")]
        public ActionResult DescriptionEdit(DrawVM vm)
        {
            foreach (Description desc in db.Descriptions.Where(d => d.ObjectTypeId == 3 && d.ObjectId == vm.DrawId))
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
    }
}