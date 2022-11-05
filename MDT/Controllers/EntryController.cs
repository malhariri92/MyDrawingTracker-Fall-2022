using MDT.Models.DTO;
using MDT.Filters;
using MDT.Models;
using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MDT.Controllers
{
    public class EntryController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GoToEntryForm(int drawId, int drawTypeId, int userId)
        {
            try
            {
                DrawEntry drawEntry = new DrawEntry();
                drawEntry.DrawId = drawId;
                drawEntry.UserId = userId;
                drawEntry.EntryCode = WebManager.GetUniqueKey(6);
                db.Entry(drawEntry).State = EntityState.Added;
                db.SaveChanges();
                drawEntry = db.DrawEntries.Where(de => de.EntryId == drawEntry.EntryId)
                    .Include(de => de.Draw)
                    .Include(de => de.User)
                    .FirstOrDefault();
                EntryVM vm = new EntryVM(drawEntry);
                return PartialView(vm);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Caught: " + e.Message);
                return RedirectToAction("ViewDraw", "Draw");
            }
        }

        public ActionResult AddEntries(int drawId, int drawTypeId, int userId, int entryCount, int entryId)
        {

            if (GetDrawEntries(drawId, userId, group.AccountBalanceLedgerId, entryCount))
            {
                ViewBag.Entries = (List<DrawEntry>)TempData["Entries"];
                TempData.Remove("Entries");
            }
            else
            {
                ViewBag.Error = (string)TempData["Error"];
                TempData.Remove("Error");
            }
           
            return PartialView();
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult AddNewEntry(int DrawId = 0)
        {
            // Get
            List<int> DrawIds = GetAvailableDrawIds(DrawId);

            // If the Id is 0 or not in the persons group, don't let them pass.
            if (DrawId == 0 || !DrawIds.Contains(DrawId))
            {
                TempData["Error"] = "Draw not found!";
                return RedirectToAction("Index", "Home");
            }

            GetUsersList();

            // Find the Draw object we are adding entries to.
            Draw draw = db.Draws.Find(DrawId);

            // Add relevant info to the view model
            EntryVM vm = new EntryVM()
            {
                DrawId = draw.DrawId,
                GameName = draw.DrawType.DrawTypeName,
                EntryCount = 1,
            };

            return View(vm);
        }

        [AdminFilter(Role = "Admin")]
        public void RemoveEntry(int id = 0)
        {
            if (RemoveDrawEntries(new List<int>() { id }, true))
            {

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminFilter(Role = "Admin")]
        public ActionResult AddNewEntry(EntryVM vm)
        {
            // Ensure the state of the model is valid.
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Draw not found!";
                return RedirectToAction("Index", "Home");
            }

            if (GetDrawEntries(vm.DrawId, vm.UserId, group.AccountBalanceLedgerId, vm.EntryCount))
            {
                ViewBag.Entries = (List<DrawEntry>)TempData["Entries"];
                TempData.Remove("Entries");
            }
            else
            {
                ViewBag.Error = (string)TempData["Error"];
                TempData.Remove("Error");
            }

            return View(vm);
        }

        private void GetUsersList()
        {
            // Convert a list of Group User objects to DdlItem objects.
            List<GroupUser> GrpUsrs = db.GroupUsers
                .Where(gu => gu.GroupId == group.GroupId)
                .ToList();

            List<DdlItem> Entries = new List<DdlItem>();
            GrpUsrs.ForEach(gu => Entries.Add(new DdlItem(gu.UserId, gu.User.UserName)));
            ViewBag.Entries = Entries;
        }

        private List<int> GetAvailableDrawIds(int DrawId)
        {
            return db.Draws
                .Where(d => d.DrawId == DrawId && d.DrawType.GroupId == group.GroupId)
                .Select(d => d.DrawId)
                .ToList();
        }
    }
}