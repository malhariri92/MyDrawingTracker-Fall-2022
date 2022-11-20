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
using System.Threading;
using System.Configuration;

namespace MDT.Controllers
{
    public class DrawController : BaseController
    {
        public ActionResult Index()
        {
            return View();
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

        [AdminFilter(Role = "Admin", Permission = "DrawTypes")]
        public ActionResult EditDrawType(int id = 0)
        {
            DrawTypeVM vm = GetDrawTypeVM(id);
            return PartialView(vm);
        }

        [HttpPost]
        [AdminFilter(Role = "Admin", Permission = "DrawTypes")]
        public ActionResult EditDrawType(DrawTypeVM vm)
        {
            DrawType dt = db.DrawTypes.Find(vm.DrawTypeId) ?? new DrawType()
            {
                GroupId = group.GroupId,
                DrawTypeName = vm.TypeName,
                IsInternal = vm.IsInternal,
                Ledger = new Ledger()
                {
                    GroupId = group.GroupId,
                    LedgerName = vm.TypeName,
                    Balance = 0.0m
                }
            };

            if (vm.DrawTypeId > 0)
            {
                vm.TypeName = dt.DrawTypeName;
                ModelState.Clear();
                for (int i = 0; i < 7; i++)
                {
                    DayOfWeek d = (DayOfWeek)i;
                    vm.Schedule.Days[i].DayName = d.ToString();
                    vm.Schedule.Days[i].Abbr = d.ToString().Substring(0, 3);
                    vm.Schedule.Days[i].DayNumber = i;
                }
                TryValidateModel(vm);
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView(vm);
            }

            dt.EntryCost = vm.EntryCost;
            dt.IsActive = vm.IsActive;
            dt.PassDrawnToNext = vm.PassDrawnToNext;
            dt.EntriesToDraw = vm.EntriesToDraw;
            dt.MaxEntriesPerUser = vm.MaxEntriesPerUser;
            dt.RemoveDrawnEntries = vm.RemoveDrawnEntries;
            dt.RemoveDrawnUsers = vm.RemoveDrawnUsers;
            dt.JoinConfirmationRequired = vm.JoinConfirmationRequired;
            dt.RefundConfirmationRequired = vm.RefundConfirmationRequired;
            dt.IsolateBalance = vm.IsolateBalance;
            dt.AllowAllocation = vm.AllowAllocation;
            dt.NumberOfDraws = vm.NumberOfDraws;
            dt.InitialUserBalance = vm.InitialUserBalance;

            db.Entry(dt).State = dt.DrawTypeId == 0 ? EntityState.Added : EntityState.Modified;
            db.SaveChanges();

            if (vm.HasSchedule)
            {
                for (int i = 0; i < vm.Schedule.Days.Count; i++)
                {
                    ScheduleDayVM schedule = vm.Schedule.Days[i];

                    if (schedule.DrawTime != null)
                    {
                        Schedule sch = new Schedule()
                        {
                            DrawTypeId = dt.DrawTypeId,
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
                db.Schedules.RemoveRange(db.Schedules.Where(s => s.DrawTypeId == dt.DrawTypeId));
            }

            db.SaveChanges();

            if (vm.DrawTypeId == 0)
            {
                return Content(Url.Action("ViewDrawType", "Draw", new { id = dt.DrawTypeId }));
            }
            return PartialView("DrawTypeRules", new DrawTypeVM(dt));

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

        [AdminFilter(Role = "Admin", Permission = "DrawTypes")]
        public ActionResult DrawTypeDescriptionEdit(int id)
        {
            DrawTypeVM vm = GetDrawTypeVM(id);

            if (id == vm.DrawTypeId)
            {
                return PartialView(vm);
            }
            return PartialView("Nope");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin", Permission = "DrawTypes")]
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

        public ActionResult JoinDrawType(int id)
        {
            DrawType dt = db.DrawTypes.Find(id);
            if (dt == null || dt.GroupId != group.GroupId)
            {
                return View("Error");
            }
            UserDrawTypeOption udto = GetUserDrawTypeOption(id, user.UserId);
            if (WebManager.IsGroupAdmin(group.GroupId, user.UserId) || !dt.JoinConfirmationRequired)
            {
                udto.IsApproved = true;
                db.Entry(udto).State = EntityState.Modified;
                db.SaveChanges();
            }

            return PartialView("DrawTypeRules", GetDrawTypeVM(id));
        }

        public ActionResult Approve(int id, int uId)
        {
            UserDrawTypeOption udto = GetUserDrawTypeOption(id, uId);
            udto.IsApproved = true;
            db.Entry(udto).State = EntityState.Modified;
            db.SaveChanges();
            return PartialView("UserRequests", GetDrawTypeVM(id).UserOptions);
        }

        public ActionResult Reject(int id, int uId)
        {
            UserDrawTypeOption udto = GetUserDrawTypeOption(id, uId);
            udto.IsApproved = true;
            db.Entry(udto).State = EntityState.Deleted;
            db.SaveChanges();
            return PartialView("UserRequests", GetDrawTypeVM(id).UserOptions);
        }


        public ActionResult UpdateAllocation(int id)
        {
            DrawType dt = db.DrawTypes.Find(id);
            if (dt == null || dt.GroupId != group.GroupId)
            {
                return PartialView("Error");
            }

            ViewBag.Users = GetDdl(db.GroupUsers);

            AllocationVM vm = new AllocationVM(GetUserDrawTypeOption(id, user.UserId), GetUserBalance(dt.LedgerId, user.UserId));
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAllocation(AllocationVM vm)
        {
            DrawType dt = db.DrawTypes.Find(vm.DrawTypeId);
            if (dt == null || dt.GroupId != group.GroupId)
            {
                Response.StatusCode = 400;
                return PartialView("Error");
            }

            if (!WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                vm.UserId = user.UserId;
            }

            Balance source = GetUserBalance(vm.Amount > 0 ? group.AccountBalanceLedgerId : dt.LedgerId, vm.UserId);
            Balance destination = GetUserBalance(vm.Amount > 0 ? dt.LedgerId : group.AccountBalanceLedgerId, vm.UserId);
            vm.Amount = Math.Abs(vm.Amount);

            if (!BalanceAvailable(source.LedgerId, vm.UserId, vm.Amount))
            {
                Response.StatusCode = 400;
                return PartialView("InsufficientFunds");
            }

            UserDrawTypeOption option = GetUserDrawTypeOption(vm.DrawTypeId, vm.UserId);

            option.PlayAll = vm.Join;
            option.MaxPlay = dt.MaxEntriesPerUser == 0 ? vm.EntriesPerDrawing : dt.MaxEntriesPerUser > vm.EntriesPerDrawing ? vm.EntriesPerDrawing : dt.MaxEntriesPerUser;
            db.Entry(option).State = EntityState.Modified;
            if (vm.Amount != 0 && CreateTransaction(vm.UserId, 8, vm.Amount, source.LedgerId, true, destination.LedgerId, true))
            {
                if (option.MaxPlay > 0)
                {
                    List<Draw> Draws = db.Draws.Where(d => d.DrawTypeId == dt.DrawTypeId && d.StartDateTime != null && d.EndDateTime < DateTime.Now).OrderBy(d => d.EndDateTime).ToList();
                    Dictionary<Draw, List<DrawEntry>> NewEntries = new Dictionary<Draw, List<DrawEntry>>();
                    Dictionary<Draw, string> Errors = new Dictionary<Draw, string>();
                    foreach (Draw draw in Draws)
                    {
                        int entryCount = draw.DrawEntries.Where(e => e.UserId == vm.UserId).Count();
                        int maxEntries = (draw.DrawOption?.MaxEntriesPerUser ?? dt.MaxEntriesPerUser) == 0 ? option.MaxPlay : (draw.DrawOption?.MaxEntriesPerUser ?? dt.MaxEntriesPerUser) > option.MaxPlay ? option.MaxPlay : (draw.DrawOption?.MaxEntriesPerUser ?? dt.MaxEntriesPerUser);

                        if (entryCount < maxEntries)
                        {
                            if (GetDrawEntries(draw.DrawId, vm.UserId, dt.LedgerId, maxEntries - entryCount))
                            {
                                NewEntries.Add(draw, (List<DrawEntry>)TempData["Entries"]);
                                TempData.Remove("Entries");
                            }
                            else
                            {
                                Errors.Add(draw, (string)TempData["Error"]);
                                TempData.Remove("Error");
                            }
                        }
                    }

                    ViewBag.Errors = Errors;
                    ViewBag.Entries = NewEntries;
                }
            }
            else
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
                return PartialView();
            }

            return PartialView();
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

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult CreateDraw(int id)
        {
            DrawType dt = GetDrawType(id);
            DrawVM vm = new DrawVM(dt);

            return PartialView("EditDraw", vm);

        }

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult EditDraw(int id)
        {
            DrawVM vm = GetDrawVM(id);
            if (id != vm.DrawId)
            {
                TempData["Error"] = "Draw not found";
                return RedirectToAction("Index", "Home");
            }

            return PartialView("EditDraw", vm);
        }



        [HttpPost]
        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult EditDraw(DrawVM vm)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == vm.DrawId)
                              .Include(d => d.DrawType)
                              .Include(d => d.DrawOption)
                              .FirstOrDefault();

            if (vm.DrawId != 0 && (draw == null || !GetDdl(db.Draws).Any(d => d.val == draw.DrawId)))
            {
                TempData["Error"] = "Draw not found";
                return Content(Url.Action("Index", "Home", null));
            }


            if (vm.DrawId == 0)
            {
                DrawType dt = db.DrawTypes.Find(vm.DrawTypeId);
                if (dt == null)
                {
                    TempData["Error"] = "Draw Type not found";
                    return Content(Url.Action("Index", "Home", null));
                }

                draw = new Draw()
                {
                    DrawTypeId = vm.DrawTypeId,
                    DrawCode = WebManager.GetUniqueKey(7)
                };

                if (dt.IsInternal)
                {
                    draw.DrawOption = new DrawOption();
                    draw.DrawOption.EntriesToDraw = dt.EntriesToDraw;
                    draw.DrawOption.MaxEntriesPerUser = dt.MaxEntriesPerUser;
                    draw.DrawOption.PassDrawnToNext = dt.PassDrawnToNext;
                    draw.DrawOption.RemoveDrawnEntries = dt.RemoveDrawnEntries;
                    draw.DrawOption.RemoveDrawnUsers = dt.RemoveDrawnUsers;
                }
            }

            draw.EndDateTime = vm.EndDate.Value + vm.EndTime.Value;

            db.Entry(draw).State = draw.DrawId == 0 ? EntityState.Added : EntityState.Modified;
            db.SaveChanges();

            if (vm.DrawId == 0)
            {
                return Content(Url.Action("ViewDraw", "Draw", new { id = draw.DrawId }));
            }

            return PartialView("DrawRules", GetDrawVM(draw.DrawId));
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

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult DrawDescriptionEdit(int id)
        {
            DrawVM vm = GetDrawVM(id);

            if (id == vm.DrawId)
            {
                return PartialView(vm);
            }
            return PartialView("Nope");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin", Permission = "Drawings")]
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

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult StartDraw(int id)
        {
            Draw draw = GetDraw(id);
            if (draw != null)
            {
                draw.StartDateTime = DateTime.Now;
                db.Entry(draw).State = EntityState.Modified;
                db.SaveChanges();

                DrawType dt = draw.DrawType;
                if (dt.AllowAllocation)
                {
                    foreach (UserDrawTypeOption udto in dt.UserDrawTypeOptions.Where(o => o.MaxPlay > 0))
                    {
                        if (GetDrawEntries(draw.DrawId, udto.UserId, dt.LedgerId, udto.MaxPlay))
                        {
                            ViewBag.Entries = (List<DrawEntry>)TempData["Entries"];
                            TempData.Remove("Entries");
                        }
                        else
                        {
                            ViewBag.Error = (string)TempData["Error"];
                            TempData.Remove("Error");
                        }
                    }
                }
            }

            return View("ViewDraw", GetDrawVM(draw?.DrawId ?? 0));
        }

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult RemoveUserEntries(int id)
        {
            Draw draw = GetDraw(id);

            if (draw == null || draw.StartDateTime == null || draw.EndDateTime < DateTime.Now)
            {
                ModalMessageVM mm = new ModalMessageVM();
                mm.Header = "Invalid Draw";
                mm.Body = $"Unable to remove entries from the selected drawing.";
                return PartialView("ModalMessage", mm);
            }

            ViewBag.Users = GetDdl(db.GroupUsers);

            EntryVM vm = new EntryVM(draw);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult RemoveUserEntries(EntryVM vm)
        {
            ModalMessageVM mm = new ModalMessageVM();
            GroupUser gu = db.GroupUsers.Find(group.GroupId, vm.UserId);
            if (gu == null)
            {
                mm.Header = "Operation failed";
                mm.Body = "Selected user not found in group.";
                return PartialView("ModalMessage", mm);
            }

            if (RemoveDrawEntries(db.DrawEntries.Where(de => de.DrawId == vm.DrawId && de.UserId == user.UserId).Take(vm.EntryCount).Select(de => de.EntryId).ToList(), true))
            {
                mm.Header = "Request Successful";
            }
            else
            {
                mm.Header = "Operation could not be completed";
            }
            mm.Body = (string)TempData["Results"];
            TempData.Remove("Results");
            mm.RedirectButton = true;
            mm.RedirectLink = Url.Action("ViewDraw", "Draw", new { id = vm.DrawId });
            mm.RedirectText = "Close";
            return PartialView("ModalMessage", mm);



        }

        public ActionResult RemoveEntries(int id)
        {
            Draw d = GetDraw(id);

            if (d == null || d.StartDateTime == null || d.EndDateTime < DateTime.Now)
            {
                ModalMessageVM mm = new ModalMessageVM();
                mm.Header = "Invalid Draw";
                mm.Body = $"Unable to add entries to the selected drawing.";
                return PartialView("ModalMessage", mm);
            }
            EntryVM vm = new EntryVM(d);

            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveEntries(EntryVM vm)
        {
            ModalMessageVM mm = new ModalMessageVM();

            if (RemoveDrawEntries(db.DrawEntries.Where(de => de.DrawId == vm.DrawId && de.UserId == user.UserId).Take(vm.EntryCount).Select(de => de.EntryId).ToList(), false))
            {
                mm.Header = "Request Successful";
                mm.RedirectButton = true;
                mm.RedirectLink = Url.Action("ViewDraw", "Draw", new { id = vm.DrawId });
                mm.RedirectText = "Close";
            }
            else
            {
                mm.Header = "Operation could not be completed";
            }
            mm.Body = (string)TempData["Results"];
            mm.HtmlBody = true;
            TempData.Remove("Results");
            return PartialView("ModalMessage", mm);
        }

        public ActionResult AddEntries(int id)
        {
            Draw d = GetDraw(id);

            if (d == null || d.StartDateTime == null || d.EndDateTime < DateTime.Now)
            {
                ModalMessageVM mm = new ModalMessageVM();
                mm.Header = "Invalid Draw";
                mm.Body = $"Unable to add entries to the selected drawing.";
                return PartialView("ModalMessage", mm);
            }
            EntryVM vm = new EntryVM(d);

            int UserEntries = db.DrawEntries.Count(e => e.DrawId == id && e.UserId == user.UserId);
            if (vm.MaxCount > 0 && UserEntries >= vm.MaxCount)
            {
                ModalMessageVM mm = new ModalMessageVM();
                mm.Header = "Entry Count Limit Reached";
                mm.Body = $"This drawing has a limit of {(vm.MaxCount == 1 ? "1 entry" : $"{vm.MaxCount} entries")}. You already have {(UserEntries == 1 ? "1 entry" : $"{UserEntries} entries")}.";
                return PartialView("ModalMessage", mm);
            }

            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEntries(EntryVM vm)
        {
            ModalMessageVM mm = new ModalMessageVM();

            if (GetDrawEntries(vm.DrawId, user.UserId, group.AccountBalanceLedgerId, vm.EntryCount))
            {
                Draw d = GetDraw(vm.DrawId);
                List<DrawEntry> entries = (List<DrawEntry>)TempData["Entries"];
                TempData.Remove("Entries");

                mm.Header = "Purchase Successful";
                mm.Body = $"Added {(entries.Count > 1 ? $"{entries.Count} entries" : "1 entry")} for {d.Title ?? $"{d.DrawType.DrawTypeName} {d.EndDateTime:yyyy-MM-dd}" }";
                mm.RedirectButton = true;
                mm.RedirectLink = Url.Action("ViewDraw", "Draw", new { id = d.DrawId });
                mm.RedirectText = "Close";

            }
            else
            {

                mm.Header = "Unable to Purchase";
                mm.Body = (string)TempData["Error"];
                TempData.Remove("Error");
            }

            return PartialView("ModalMessage", mm);
        }

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult AddUserEntries(int id)
        {
            Draw draw = GetDraw(id);

            if (draw == null || draw.StartDateTime == null || draw.EndDateTime < DateTime.Now)
            {
                ModalMessageVM mm = new ModalMessageVM();
                mm.Header = "Invalid Draw";
                mm.Body = $"Unable to add entries to the selected drawing.";
                return PartialView("ModalMessage", mm);
            }

            ViewBag.Users = GetDdl(db.GroupUsers);

            EntryVM vm = new EntryVM(draw);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult AddUserEntries(EntryVM vm)
        {
            ModalMessageVM mm = new ModalMessageVM();
            GroupUser gu = db.GroupUsers.Find(group.GroupId, vm.UserId);
            if (gu == null)
            {
                mm.Header = "Operation failed";
                mm.Body = "Selected user not found in group.";
                return PartialView("ModalMessage", mm);
            }

            if (GetDrawEntries(vm.DrawId, vm.UserId, group.AccountBalanceLedgerId, vm.EntryCount))
            {
                Draw d = GetDraw(vm.DrawId);
                List<DrawEntry> entries = (List<DrawEntry>)TempData["Entries"];
                TempData.Remove("Entries");

                mm.Header = "Purchase Successful";
                mm.Body = $"Added {(entries.Count > 1 ? $"{entries.Count} entries" : "1 entry")} for {d.Title ?? $"{d.DrawType.DrawTypeName} {d.EndDateTime:yyyy-MM-dd}" }";
                mm.RedirectButton = true;
                mm.RedirectLink = Url.Action("ViewDraw", "Draw", new { id = d.DrawId });
                mm.RedirectText = "Close";

            }
            else
            {

                mm.Header = "Unable to Purchase";
                mm.Body = (string)TempData["Error"];
                TempData.Remove("Error");
            }

            return PartialView("ModalMessage", mm);
        }



        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult RemovalRequests(int drawId, int userId)
        {
            List<EntryVM> entries = db.DrawEntries.Where(e => e.DrawId == drawId && e.UserId == userId && e.PendingRemoval).ToList().Select(e => new EntryVM(e)).ToList();
            ModalMessageVM mm = new ModalMessageVM();
            if (!entries.Any())
            {
                mm.Header = "Operation failed";
                mm.Body = "Entries not found.";
                return PartialView("ModalMessage", mm);
            }
            return PartialView(entries);
        }

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult RejectRemoval(int id)
        {
            DrawEntry entry = db.DrawEntries.Find(id);
            
            ModalMessageVM mm = new ModalMessageVM();
            if (entry == null)
            {
                mm.Header = "Operation failed";
                mm.Body = "Entry not found.";
                return PartialView("ModalMessage", mm);
            }

            int d = entry.DrawId;
            entry.PendingRemoval = false;
            db.Entry(entry).State = EntityState.Modified;
            db.SaveChanges();

            mm.Header = "Operation Sucessful";
            mm.Body = "Removal request has been rejected.";
            mm.RedirectButton = true;
            mm.RedirectLink = Url.Action("ViewDraw", "Draw", new { id = d });
            mm.RedirectText = "Close";
            return PartialView("ModalMessage", mm);
        }

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult ApproveRemoval(int id)
        {
            DrawEntry entry = db.DrawEntries.Find(id);
            ModalMessageVM mm = new ModalMessageVM();
            if (entry == null)
            {
                mm.Header = "Operation failed";
                mm.Body = "Entry not found.";
                return PartialView("ModalMessage", mm);
            }

            if (!entry.PendingRemoval)
            {
                mm.Header = "Operation failed";
                mm.Body = "Entry removal has not been requests.";
                return PartialView("ModalMessage", mm);
            }
            int d = entry.DrawId;
            string code = entry.EntryCode;
            db.Entry(entry).State = EntityState.Deleted;
            db.SaveChanges();

            mm.Header = "Operation Sucessful";
            mm.Body = $"Entry {code} has been removed.";
            mm.RedirectButton = true;
            mm.RedirectLink = Url.Action("ViewDraw", "Draw", new { id = d });
            mm.RedirectText = "Close";
            return PartialView("ModalMessage", mm);
        }


        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult ApproveAllRemoval(int drawId, int userId)
        {
            List<int> ids = db.DrawEntries.Where(e => e.DrawId == drawId && e.UserId == userId && e.PendingRemoval).Select(e => e.EntryId).ToList();
            ModalMessageVM mm = new ModalMessageVM();
            if (!ids.Any())
            {
                mm.Header = "Operation failed";
                mm.Body = "Entries not found.";
                return PartialView("ModalMessage", mm);
            }

            if (RemoveDrawEntries(ids, true))
            {
                mm.Header = "Request Successful";
            }
            else
            {
                mm.Header = "Operation could not be completed";
            }

            mm.Body = (string)TempData["Results"];
            TempData.Remove("Results");
            mm.HtmlBody = true;
            mm.RedirectButton = true;
            mm.RedirectLink = Url.Action("ViewDraw", "Draw", new { id = drawId });
            mm.RedirectText = "Close";
            return PartialView("ModalMessage", mm);
        }

        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult RejectAllRemoval(int drawId, int userId)
        {
            List<DrawEntry> entries = db.DrawEntries.Where(e => e.DrawId == drawId && e.UserId == userId && e.PendingRemoval).ToList();
            ModalMessageVM mm = new ModalMessageVM();
            if (!entries.Any())
            {
                mm.Header = "Operation failed";
                mm.Body = "Entries not found.";
                return PartialView("ModalMessage", mm);
            }

            foreach (DrawEntry entry in entries)
            {
                entry.PendingRemoval = false;
                db.Entry(entry).State = EntityState.Modified;
            }
            db.SaveChanges();

            mm.Header = "Operation Sucessful";
            mm.Body = "Removal requests have all been rejected.";
            mm.RedirectButton = true;
            mm.RedirectLink = Url.Action("ViewDraw", "Draw", new { id = drawId });
            mm.RedirectText = "Close";
            return PartialView("ModalMessage", mm);
        }

        public ActionResult Results(int id)
        {
            DrawVM vm = GetDrawVM(id);
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin", Permission = "Drawings")]
        public ActionResult Results(DrawVM vm)
        {
            Draw draw = GetDraw(vm.DrawId);

            draw.Results = vm.Results;
            db.Entry(draw).State = EntityState.Modified;
            db.SaveChanges();

            vm = GetDrawVM(draw.DrawId);
            return PartialView(vm);
        }
    }
}