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
            TransactionListVM vm = GetTransactionListVM();
            return View(vm);
        }

        public ActionResult Transactions()
        {
            TransactionListVM vm = GetTransactionListVM();
            return PartialView(vm);
        }

        public ActionResult TransactionsPending()
        {
            TransactionListVM vm = GetTransactionListVM();
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
                Response.StatusCode = 400;
                return View(vm);
            }
            CreateTransaction(vm.UserId, vm.TransactionTypeId, vm.Amount, 0, false, group.AccountBalanceLedgerId, true);

            TransactionListVM tlvm = GetTransactionListVM();
            ViewBag.Message = "Transaction has been added.";
            return PartialView("Transactions", tlvm);
        }

        [AdminFilter(Role = "Admin", Permission = "Transactions")]
        public ActionResult Accept(int id)
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
            TransactionListVM tlvm = GetTransactionListVM();
            return PartialView("TransactionsPending", tlvm);
        }

        [AdminFilter(Role = "Admin", Permission = "Transactions")]
        public ActionResult Reject(int id)
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

            TransactionListVM tlvm = GetTransactionListVM();
            return PartialView("TransactionsPending", tlvm);
        }

        public ActionResult ReportNewTransaction()
        {
            TransactionVM vm = new TransactionVM();
            vm.UserId = user.UserId;
            ViewBag.TransactionTypes = GetDdl(db.TransactionTypes);
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReportNewTransaction(TransactionVM vm)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView(vm);
            }

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

            db.Entry(entry).State = EntityState.Added;
            db.SaveChanges();

            TransactionListVM tlvm = GetTransactionListVM();
            ViewBag.Message = "Pending transaction has been reported.";
            return PartialView("TransactionsPending", tlvm);
        }

        private TransactionListVM GetTransactionListVM()
        {
            return new TransactionListVM(GetTransactions(), GetPending());
        }

        private List<Transaction> GetTransactions()
        {
            bool admin = WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId) || WebManager.HasPermission(user.CurrentGroupId, user.UserId, "Transactions");
            return db.Transactions.Where(t => t.GroupId == group.GroupId && (admin || t.UserId == user.UserId))
                                  .Include(t => t.User)
                                  .Include(t => t.TransactionType)
                                  .Include(t => t.FromLedger)
                                  .Include(t => t.ToLedger)
                                  .Include(t => t.User)
                                  .ToList();
        }

        private List<PendingTransaction> GetPending()
        {
            bool admin = WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId) || WebManager.HasPermission(user.CurrentGroupId, user.UserId, "Transactions");
            return db.PendingTransactions.Where(t => t.GroupId == group.GroupId && (admin || t.UserId == user.UserId))
                                         .Include(t => t.User)
                                         .Include(t => t.TransactionType)
                                         .Include(t => t.FromLedger)
                                         .Include(t => t.ToLedger)
                                         .Include(t => t.User)
                                         .ToList();
        }
    }
}