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

        [AdminFilter(Role  = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void RemoveEntry(int EntryId = 0)
        {
            DrawEntry entry = db.DrawEntries.Find(EntryId);

            if (entry != null)
            {
                const int transType = 7;

                Balance balance = db.Balances.Where(b => b.UserId == entry.UserId).FirstOrDefault();
                Draw draw = db.Draws.Find(entry.DrawId);
                balance.CurrentBalance -= draw.DrawType.EntryCost;

                Transaction trans = new Transaction()
                {
                    DestinationLedger = 1,
                    SourceLedger = 1,
                    Amount = draw.DrawType.EntryCost,
                    DrawId = entry.DrawId,
                    TransactionDateTime = DateTime.Now,
                    TransactionTypeId = transType,
                    UserId = entry.UserId,
                };

                db.Entry(balance).State = EntityState.Modified;
                db.Entry(trans).State = EntityState.Added;
                db.Entry(entry).State = EntityState.Deleted;
                db.SaveChanges();
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

            // Query the database for relevant objects we need.
            Draw draw = db.Draws.Find(vm.DrawId);
            Balance balance = db.Balances.Where(b => b.UserId == vm.UserId).FirstOrDefault();

            // Get the total cost of the transaction
            decimal totalCost = vm.EntryCount * draw.DrawType.EntryCost;

            //Add the list of possible users to ViewBag.
            GetUsersList();

            // Check if the number of entries is not negative and if the selected user has enough balance.
            if (balance == null || balance.CurrentBalance < totalCost)
            {
                ViewBag.tooMuch = true;
                return View(vm);
             }
            else if (vm.EntryCount <= 0)
            {
                ViewBag.isNeg = true;
                GetUsersList();
                return View(vm);
            } 

            // Find the user associated with the entry.
            User user = db.Users.Find(vm.UserId);

            // Constant int that points towards the correct transaction type id.
            const int transType = 6;

            // Create the transaction.
            Transaction trans = new Transaction()
            {
                SourceLedger = 1,
                DestinationLedger = 1,
                Amount = totalCost,
                TransactionDateTime = DateTime.Now,
                DrawId = draw.DrawId,
                UserId = user.UserId,
                TransactionTypeId = transType,
            };

            // Set the state of the transaction to added.
            db.Entry(trans).State = EntityState.Added;

            // Subtract the total cost of the entry purchase from the balance object and set its entity state to modified.
            balance.CurrentBalance -= totalCost;
            db.Entry(balance).State = EntityState.Modified;

            /* Loop through vm.EntryCount number of times, creating a new DrawEntry object with a unique key with a length of 6.
               Set the new DrawEntry object's state to Added*/
            for (int i = 0; i < vm.EntryCount; i++) 
            {
                DrawEntry entry = new DrawEntry()
                {
                    EntryCode = WebManager.GetUniqueKey(6),
                    DrawId = draw.DrawId,
                    UserId = user.UserId,
                };

                db.Entry(entry).State = EntityState.Added;
            }

            // Save the accumulated changes made to the database.
            db.SaveChanges();

            // All entries have been added to the database, display a success message to the admin.
            ViewBag.SuccessMessage = 
                $"Successfully added {vm.EntryCount} entries for the user {user.UserName}!";

            // Return to the view again.
            return View(new EntryVM()
            {
                DrawId = draw.DrawId,
                GameName = draw.DrawType.DrawTypeName,
                EntryCount = 1,
            });
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
                .Where(d => d.DrawId == DrawId && d.DrawType.GroupDrawTypes.Any(g => g.GroupId == group.GroupId))
                .Select(d => d.DrawId)
                .ToList();
        }
    }
}