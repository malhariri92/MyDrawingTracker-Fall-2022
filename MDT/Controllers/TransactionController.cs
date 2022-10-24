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
        [AdminFilter(Role = "Admin")]
        public ActionResult AddNewTransaction()
        {
            TransactionVM vm = new TransactionVM();
            ViewBag.TransactionTypes = GetDdl(db.TransactionTypes);
            ViewBag.Users = GetDdl(db.GroupUsers);
            return View(vm);
        }

        [AdminFilter(Role = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewTransaction(TransactionVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            Balance userBalance = db.Balances.Find(group.AccountBalanceLedgerId, vm.UserId);
            if (userBalance == null)
            {
                userBalance = new Balance()
                {
                    LedgerId = group.AccountBalanceLedgerId,
                    UserId = vm.UserId,
                    CurrentBalance = 0.0m
                };
                db.Balances.Add(userBalance);
                db.SaveChanges();
            }

            Transaction entry = new Transaction()
            {
                TransactionTypeId = vm.TransactionTypeId,
                Amount = vm.Amount,
                UserId = vm.UserId,
                GroupId = group.GroupId,
                TransactionDateTime = DateTime.Now,
                SourceLedger = 1,
                DestinationLedger = group.AccountBalanceLedgerId
            };

            db.Transactions.Add(entry);

            userBalance.CurrentBalance += vm.Amount;
            db.Entry(userBalance).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {

            List<TransactionDTO> tld = new List<TransactionDTO>();
            using (var db = new DbEntities())
            {
                if (WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId))
                {
                    List<Transaction> transactions = db.Transactions.Where(t => t.GroupId == group.GroupId).ToList();
                    foreach (Transaction t in transactions)
                    {
                        tld.Add(new TransactionDTO(t));
                    }

                }
                else
                {
                    List<Transaction> transactions = db.Transactions.Where(t => t.UserId == user.UserId && t.GroupId == group.GroupId).ToList();
                    foreach (Transaction t in transactions)
                    {
                        tld.Add(new TransactionDTO(t));
                    }
                }
                return PartialView(tld);
            }
        }

        [LoginFilter]
        public ActionResult ReportNewTransaction()
        {
            TransactionVM vm = new TransactionVM();
            vm.UserId = user.UserId;
            ViewBag.TransactionTypes = GetDdl(db.TransactionTypes);
            return View(vm);
        }

        [LoginFilter]
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
                    SourceLedger = 1,
                    DestinationLedger = group.AccountBalanceLedgerId
                };

                db.PendingTransactions.Add(entry);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }
    }
}