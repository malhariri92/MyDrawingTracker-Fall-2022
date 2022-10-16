using MDT.Models.DTO;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int drawId, int drawTypeId, int userId)
        {
            try
            {
                using (var db = new DbEntities())
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
                    return View(vm);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Caught: " + e.Message);
                return RedirectToAction("ViewDraw", "Draw");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEntries(EntryVM vm)
        {
            System.Diagnostics.Debug.WriteLine("In AddEntries Post");
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState invalid in AddEntries");
                return View(vm);
            }

            try
            {
                using (var db = new DbEntities())
                {
                    DrawType drawType = db.DrawTypes.Find(vm.DrawTypeId);
                    if (drawType == null)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Could not find proper drawing. Please contact your administrator.";
                        DrawEntry remove = db.DrawEntries.Find(vm.EntryId);
                        if (remove != null)
                        {
                            db.Entry(remove).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        return View(vm);
                    }

                    if (drawType.MaxEntriesPerUser < vm.EntryCount)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "You are attempting to purchase more entries than allowed (" + drawType.MaxEntriesPerUser + ")";
                        DrawEntry remove = db.DrawEntries.Find(vm.EntryId);
                        if (remove != null)
                        {
                            db.Entry(remove).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        return View(vm);
                    }

                    User user = db.Users.Find(vm.UserId);
                    if (user == null)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Failed to find current user. Please contact your administrator.";
                        DrawEntry remove = db.DrawEntries.Find(vm.EntryId);
                        if (remove != null)
                        {
                            db.Entry(remove).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        return View(vm);
                    }

                    GroupDrawType gdt = db.GroupDrawTypes.Where(g => g.DrawTypeId == vm.DrawTypeId && g.GroupId == user.CurrentGroupId).FirstOrDefault();
                    if (gdt == null)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Could not find your group draw type. Please contact your administrator.";
                        DrawEntry remove = db.DrawEntries.Find(vm.EntryId);
                        if (remove != null)
                        {
                            db.Entry(remove).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        return View(vm);
                    }
                    if (!gdt.IsActive)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Inactive group draw type. Please contact your administrator.";
                        DrawEntry remove = db.DrawEntries.Find(vm.EntryId);
                        if (remove != null)
                        {
                            db.Entry(remove).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        return View(vm);
                    }
                    Balance userBalance = db.Balances.Where(b => b.UserId == vm.UserId && b.LedgerId == gdt.LedgerId).First();
                    if (userBalance == null)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Could not find your balance. Please contact your administrator.";
                        DrawEntry remove = db.DrawEntries.Find(vm.EntryId);
                        if (remove != null)
                        {
                            db.Entry(remove).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        return View(vm);
                    }
                    decimal totalCost = (decimal)vm.EntryCount * drawType.EntryCost;
                    if (totalCost > userBalance.CurrentBalance)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Not enough funds";
                        DrawEntry remove = db.DrawEntries.Find(vm.EntryId);
                        if (remove != null)
                        {
                            db.Entry(remove).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        return View(vm);
                    }

                    decimal limit = drawType.InitialUserBalance - ((decimal)drawType.MaxEntriesPerUser * drawType.EntryCost);
                    if ((userBalance.CurrentBalance - totalCost) < limit)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "You are attempting to purchase more entries than allowed (" + drawType.MaxEntriesPerUser + ")";
                        DrawEntry remove = db.DrawEntries.Find(vm.EntryId);
                        if (remove != null)
                        {
                            db.Entry(remove).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        return View(vm);
                    }

                    Transaction newTransaction = new Transaction
                    {
                        TransactionTypeId = 6,
                        Amount = totalCost,
                        UserId = user.UserId,
                        TransactionDateTime = System.DateTime.Now,
                        DrawId = vm.DrawId,
                        SourceLedger = gdt.LedgerId,
                        DestinationLedger = gdt.LedgerId
                    };
                    db.Entry(newTransaction).State = EntityState.Added;
                    db.SaveChanges();
                    newTransaction = db.Transactions.Where(t => t.TransactionId == newTransaction.TransactionId)
                        .Include(t => t.Draw)
                        .Include(t => t.User)
                        .Include(t => t.ToLedger)
                        .Include(t => t.FromLedger)
                        .Include(t => t.TransactionType)
                        .FirstOrDefault();

                    userBalance.CurrentBalance = userBalance.CurrentBalance - totalCost;
                    db.Entry(userBalance).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                vm.Success = false;
                vm.Error = true;
                vm.Message = "User Entry Exception: \n" + ex.Message;
                return View(vm);
            }

            vm.Success = true;
            vm.Error = false;
            return View(vm);
        }
    }
}