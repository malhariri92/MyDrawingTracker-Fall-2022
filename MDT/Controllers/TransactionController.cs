using MDT.Models;
using MDT.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MDT.Filters;
using MDT.ViewModels;

namespace MDT.Controllers
{
    public class TransactionController : BaseController
    {
        public ActionResult Index()
        {
            bool admin = WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId) || WebManager.HasPermission(user.CurrentGroupId, user.UserId, "Transactions");
            TransactionListVM vm = new TransactionListVM(db.Transactions.Where(t => t.GroupId == group.GroupId && (admin || t.UserId == user.UserId)).ToList(),
                                                         db.PendingTransactions.Where(pt => pt.GroupId == group.GroupId && (admin || pt.UserId == user.UserId)).ToList());
            return View(vm);
        }

        public ActionResult Transactions()
        {
            bool admin = WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId) || WebManager.HasPermission(user.CurrentGroupId, user.UserId, "Transactions");
            TransactionListVM vm = new TransactionListVM(db.Transactions.Where(t => t.GroupId == group.GroupId && (admin || t.UserId == user.UserId)).ToList(),
                                                         db.PendingTransactions.Where(pt => pt.GroupId == group.GroupId && (admin || pt.UserId == user.UserId)).ToList());

            return PartialView(vm);
        }

        public ActionResult TransactionsPending()
        {
            bool admin = WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId) || WebManager.HasPermission(user.CurrentGroupId, user.UserId, "Transactions");
            TransactionListVM vm = new TransactionListVM(db.Transactions.Where(t => t.GroupId == group.GroupId && (admin || t.UserId == user.UserId)).ToList(),
                                                         db.PendingTransactions.Where(pt => pt.GroupId == group.GroupId && (admin || pt.UserId == user.UserId)).ToList());

            return PartialView(vm);
        }

        [AdminFilter(Role = "Admin", Permission = "Transactions")]
        public ActionResult AddNewTransaction()
        {
            TransactionVM vm = new TransactionVM();
            ViewBag.TransactionTypes = GetDdl(db.TransactionTypes);
            ViewBag.Users = GetDdl(db.GroupUsers);
            return PartialView(vm);
        }

        [AdminFilter(Role = "Admin", Permission = "Transactions")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewTransaction(TransactionVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            CreateTransaction(vm.UserId, vm.TransactionTypeId, vm.Amount, 0, false, group.AccountBalanceLedgerId, true);

            TransactionListVM tlvm = new TransactionListVM(db.Transactions.Where(t => t.GroupId == group.GroupId).ToList(),
                                                           db.PendingTransactions.Where(pt => pt.GroupId == group.GroupId).ToList());
            ViewBag.Message = "Transaction has been added.";
            return PartialView("Transactions", tlvm);
        }

        [AdminFilter(Role = "Admin", Permission = "Transactions")]
        public ActionResult AcceptTransaction(int id)
        {
            PendingTransaction pending = db.PendingTransactions.Find(id);
            if (pending == null || pending.GroupId != group.GroupId)
            {
                ViewBag.Error = "Invalid pending transaction";
             
            }
            else
            {
                CreateTransaction(pending.UserId, pending.TransactionTypeId, pending.Amount, pending.SourceLedger, false, pending.DestinationLedger, true);
                db.Entry(pending).State = EntityState.Deleted;
                db.SaveChanges();
                ViewBag.Message = "Pending transaction approved";
                ViewBag.RefreshTransactions = true;
            }
            TransactionListVM tlvm = new TransactionListVM(db.Transactions.Where(t => t.GroupId == group.GroupId).ToList(),
                                                           db.PendingTransactions.Where(pt => pt.GroupId == group.GroupId).ToList());
            return PartialView("TransactionsPending", tlvm);
        }

        [AdminFilter(Role = "Admin", Permission = "Transactions")]
        public ActionResult RejectTransaction(int id)
        {
            PendingTransaction pending = db.PendingTransactions.Find(id);
            if (pending == null || pending.GroupId != group.GroupId)
            {
                ViewBag.Error = "Invalid pending transaction";
            }
            else
            {
                db.Entry(pending).State = EntityState.Deleted;
                db.SaveChanges();
                ViewBag.Message = "Pending transaction rejected";
            }

            bool admin = WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId) || WebManager.HasPermission(user.CurrentGroupId, user.UserId, "Transactions");
            TransactionListVM tlvm = new TransactionListVM(db.Transactions.Where(t => t.GroupId == group.GroupId).ToList(),
                                                           db.PendingTransactions.Where(pt => pt.GroupId == group.GroupId).ToList());
            return PartialView("TransactionsPending", tlvm);
        }

        public ActionResult ReportNewTransaction()
        {
            TransactionVM vm = new TransactionVM();
            vm.UserId = user.UserId;
            ViewBag.TransactionTypes = GetDdl(db.TransactionTypes);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReportNewTransaction(TransactionVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            else
            {
                PendingTransaction entry = new PendingTransaction()
                {
                    TransactionTypeId = vm.TransactionTypeId,
                    Amount = vm.Amount,
                    UserId = user.UserId,
                    GroupId = group.GroupId,
                    TransactionDateTime = DateTime.Now,
                    SourceLedger = 0,
                    DestinationLedger = group.AccountBalanceLedgerId
                };

                db.PendingTransactions.Add(entry);
                db.SaveChanges();

                TransactionListVM tlvm = new TransactionListVM(db.Transactions.Where(t => t.GroupId == group.GroupId &&  t.UserId == user.UserId).ToList(),
                                                               db.PendingTransactions.Where(pt => pt.GroupId == group.GroupId && pt.UserId == user.UserId).ToList());
                ViewBag.Message = "Pending transaction has been reported.";
                return PartialView("TransactionsPending", tlvm);
            }
        }
    }
}