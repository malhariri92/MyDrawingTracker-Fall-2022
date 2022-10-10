using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using MDT.Models;

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
                vm.Schedule = new ScheduleVM();
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
    }
}