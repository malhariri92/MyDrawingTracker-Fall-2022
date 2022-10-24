using MDT.ViewModels;
using MDT.Filters;
using MDT.Models;
using MDT.Models.DTO;
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

        [AdminFilter(Role = "Admin")]
        public ActionResult EditDrawType(int id = 0)
        {
            DrawTypeVM vm = GetDrawTypeVM(id);
            if (Request.IsAjaxRequest())
            {
                return PartialView(vm);
            }
            else
            {
                return View(vm);
            }

        }

        [HttpPost]
        [AdminFilter(Role = "Admin")]
        public ActionResult EditDrawType(DrawTypeVM vm)
        {
            DrawType dt = db.DrawTypes.Find(vm.DrawTypeId) ?? new DrawType();
            if (vm.DrawTypeId > 0)
            {
                vm.TypeName = dt.DrawTypeName;
                ModelState.Clear();
                TryValidateModel(vm);
            }
            if (ModelState.IsValid)
            {
                dt.DrawTypeName = vm.TypeName;
                dt.EntryCost = vm.EntryCost;
                dt.IsActive = vm.IsActive;
                dt.IsInternal = vm.IsInternal;
                dt.PassDrawnToNext = vm.PassDrawnToNext;
                dt.PassUndrawnToNext = vm.PassUndrawnToNext;
                dt.EntriesToDraw = vm.EntriesToDraw;
                dt.MaxEntriesPerUser = vm.MaxEntriesPerUser;
                dt.RemoveDrawnEntries = vm.RemoveDrawnEntries;
                dt.RemoveDrawnUsers = vm.RemoveDrawnUsers;
                dt.JoinConfirmationRequired = vm.JoinConfirmationRequired;
                dt.RefundConfirmationRequired = vm.RefundConfirmationRequired;
                dt.AutoDraw = vm.AutoDraw;
                dt.NumberOfDraws = vm.NumberOfDraws;
                dt.InitialUserBalance = vm.InitialUserBalance;

                if (dt.DrawTypeId == 0)
                {
                    db.Entry(dt).State = EntityState.Added;
                    Ledger gl = new Ledger()
                    {
                        GroupId = group.GroupId,
                        LedgerName = dt.DrawTypeName,
                        Balance = 0.0m
                    };

                    db.Entry(gl).State = EntityState.Added;
                    db.SaveChanges();

                    GroupDrawType gdt = new GroupDrawType()
                    {
                        DrawTypeId = dt.DrawTypeId,
                        IsActive = true,
                        GroupId = group.GroupId,
                        LedgerId = gl.LedgerId
                    };

                    db.Entry(gdt).State = EntityState.Added;
                }
                else
                {
                    db.Entry(dt).State = EntityState.Modified;
                }

                
                if (vm.HasSchedule)
                {
                    for (int i = 0; i < vm.Schedule.Days.Count; i++)
                    {
                        ScheduleDayVM schedule = vm.Schedule.Days[i];

                        if (schedule.DrawTime != null)
                        {
                            Schedule sch = new Schedule()
                            {
                                DrawTypeId = vm.DrawTypeId,
                                DayOfWeek = i,
                                Time = schedule.DrawTime.Value
                            };

                            if (db.Schedules.FirstOrDefault(s => s.DrawTypeId == sch.DrawTypeId && s.DayOfWeek == sch.DayOfWeek) != null)
                            {
                                sch = db.Schedules.FirstOrDefault(s => s.DrawTypeId == sch.DrawTypeId && s.DayOfWeek == sch.DayOfWeek);
                                sch.Time = schedule.DrawTime.Value;
                                db.Entry(sch).State = EntityState.Modified;
                            }
                            else
                            {
                                db.Schedules.Add(sch);
                            }
                        }
                    }
                }
                else
                {
                    db.Schedules.RemoveRange(db.Schedules.Where(s => s.DrawTypeId == vm.DrawTypeId));
                }

                db.SaveChanges();
                return View("ViewDrawType", new DrawTypeVM(dt));
            }
            return View(vm);
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult EditDraw(int id)
        {
            DrawVM vm = GetDrawVM(id);
            if (id != vm.DrawId)
            {
                TempData["Error"] = "Draw not found";
                return RedirectToAction("Index", "Home");
            }

            if (db.DrawTypes.Find(vm.DrawTypeId).IsInternal)
            {
                return PartialView("EditDraw", vm);
            }
            else
            {
                return PartialView("EditExtDraw", vm);
            }

        }

        [AdminFilter(Role = "Admin")]
        public ActionResult CreateDraw(int id)
        {
            DrawType dt = GetDrawType(id);
            DrawVM vm = new DrawVM(dt);
            if (dt.IsInternal)
            {
                return PartialView("EditDraw", vm);
            }
            else
            {
                return PartialView("EditExtDraw", vm);
            }
        }



        [HttpPost]
        [AdminFilter(Role = "Admin")]
        public ActionResult EditDraw(DrawVM vm)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == vm.DrawId)
                              .Include(d => d.DrawType)
                              .Include(d => d.DrawType.GroupDrawTypes)
                              .Include(d => d.DrawOption)
                              .FirstOrDefault();

            if (vm.DrawId != 0 && (draw == null || !GetDdl(db.Draws).Any(d => d.val == draw.DrawId)))
            {
                TempData["Error"] = "Draw not found";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    return View(vm);
                }

                draw = new Draw()
                {
                    DrawTypeId = vm.DrawTypeId
                };

                draw.DrawOption = new DrawOption();
            }

            draw.EndDateTime = vm.EndDate;
            draw.DrawOption.AutoDraw = vm.AutoDraw;
            draw.DrawOption.EntriesToDraw = vm.EntriesToDraw;
            draw.DrawOption.MaxEntriesPerUser = vm.MaxEntriesPerUser;
            draw.DrawOption.RefundConfirmationRequired = vm.RefundConfirmationRequired;
            draw.DrawOption.JoinConfirmationRequired = vm.JoinConfirmationRequired;
            draw.DrawOption.PassDrawnToNext = vm.PassDrawnToNext;
            draw.DrawOption.PassUndrawnToNext = vm.PassUndrawnToNext;
            draw.DrawOption.RemoveDrawnEntries = vm.RemoveDrawnEntries;
            draw.DrawOption.RemoveDrawnUsers = vm.RemoveDrawnUsers;


            db.Entry(draw).State = draw.DrawId == 0 ? EntityState.Added : EntityState.Modified;
            db.SaveChanges();

            return View("ViewDraw", GetDrawVM(draw.DrawId));
        }

        [HttpPost]
        [AdminFilter(Role = "Admin")]
        public ActionResult EditExtDraw(DrawVM vm)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == vm.DrawId)
                              .Include(d => d.DrawType)
                              .Include(d => d.DrawType.GroupDrawTypes)
                              .Include(d => d.DrawOption)
                              .FirstOrDefault();

            if (vm.DrawId != 0 && (draw == null || !GetDdl(db.Draws).Any(d => d.val == draw.DrawId)))
            {
                TempData["Error"] = "Draw not found";
                return RedirectToAction("Index", "Home");
            }
            else
            {

                draw = new Draw()
                {
                    DrawTypeId = vm.DrawTypeId
                };

            }

            draw.EndDateTime = vm.EndDate;

            db.Entry(draw).State = draw.DrawId == 0 ? EntityState.Added : EntityState.Modified;
            db.SaveChanges();



            return View("ViewDraw", vm);
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult StartDraw(int id)
        {
            Draw draw = GetDraw(id);
            if (draw != null)
            {
                draw.StartDateTime = DateTime.Now;
                db.Entry(draw).State = EntityState.Modified;
                db.SaveChanges();
            }

            return PartialView("DrawRules", GetDrawVM(draw?.DrawId ?? 0));
        }

        public ActionResult ViewDraw(int id)
        {
            DrawVM vm = GetDrawVM(id);
            if (id != vm.DrawId)
            {
                TempData["Error"] = "Draw not found";
                return RedirectToAction("Index", "Home");
            }

            return View(vm);

        }

        public ActionResult ViewDrawType(int id)
        {
            DrawTypeVM vm = GetDrawTypeVM(id);
            if (id != vm.DrawTypeId)
            {
                TempData["Error"] = "Draw Type not found";
                return RedirectToAction("Index", "Home");
            }

            return View(vm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin")]
        public ActionResult DrawDescriptionEdit(DrawVM vm)
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


            Draw draw = GetDraw(vm.DrawId);
            draw.Title = vm.Title;
            db.Entry(draw).State = EntityState.Modified;


            db.SaveChanges();

            return RedirectToAction("ViewDraw", new { id = vm.DrawId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin")]
        public ActionResult DrawTypeDescriptionEdit(DrawTypeVM vm)
        {
            foreach (Description desc in db.Descriptions.Where(d => d.ObjectTypeId == 2 && d.ObjectId == vm.DrawTypeId))
            {
                db.Entry(desc).State = EntityState.Deleted;
            }
            db.SaveChanges();

            vm.Descriptions = vm.Descriptions.Where(d => d.IsActive).ToList();


            int sort = 1;
            foreach (Description d in vm.Descriptions)
            {
                d.ObjectTypeId = 2;
                d.ObjectId = vm.DrawTypeId;
                d.SortOrder = sort;
                db.Entry(d).State = EntityState.Added;
                sort++;
            }

            DrawType dt = GetDrawType(vm.DrawTypeId);
            dt.DrawTypeName = vm.TypeName;
            db.Entry(dt).State = EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("ViewDrawType", new { id = vm.DrawTypeId });
        }

        public ActionResult DrawDescription(int id)
        {
            DrawVM vm = GetDrawVM(id);

            if (id == vm.DrawId)
            {
                return PartialView(vm);
            }
            return PartialView("Nope");
        }

        public ActionResult DrawTypeDescription(int id)
        {
            DrawTypeVM vm = GetDrawTypeVM(id);

            if (id == vm.DrawTypeId)
            {
                return PartialView(vm);
            }
            return PartialView("Nope");
        }


        [AdminFilter(Role = "Admin")]
        public ActionResult DrawDescriptionEdit(int id)
        {
            DrawVM vm = GetDrawVM(id);

            if (id == vm.DrawId)
            {
                return PartialView(vm);
            }
            return PartialView("Nope");
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult DrawTypeDescriptionEdit(int id)
        {
            DrawTypeVM vm = GetDrawTypeVM(id);

            if (id == vm.DrawTypeId)
            {
                return PartialView(vm);
            }
            return PartialView("Nope");
        }
    }
}