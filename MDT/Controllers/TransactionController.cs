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

        [AdminFilter(Role = "Admin")]
        public ActionResult ViewPendingTransactions()
        {
            List<PendingTransactionVM> tld = new List<PendingTransactionVM>();
            using (var db = new DbEntities())
            {
                if (WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId))
                {
                    List<User> ml = db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId).Select(gu => gu.User).ToList();
                    foreach (User u in ml)
                    {
                        List<PendingTransaction> transactions = db.PendingTransactions.Where(t => t.UserId == u.UserId).ToList();
                        foreach (PendingTransaction t in transactions)
                        {
                            tld.Add(new PendingTransactionVM(t));
                        }
                    }
                }
                return PartialView(tld);
            }
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult AcceptTransaction(int PendTransactionId)
        {
            var transaction = db.PendingTransactions.Where(p => p.PendingTransactionId == PendTransactionId).FirstOrDefault();
            var foundType = db.TransactionTypes.Where(t => t.TransactionTypeId == transaction.TransactionTypeId).FirstOrDefault();
            if (foundType.IsDebit)
            {
                var userBalance = db.Balances.Where(b => b.UserId == transaction.UserId && b.LedgerId == 2).FirstOrDefault();
                if (userBalance == null)
                {
                    Balance bal = new Balance()
                    {
                        UserId = transaction.UserId,
                        LedgerId = 2,
                        CurrentBalance = transaction.Amount
                    };
                    db.Balances.Add(bal);
                }

                else
                {
                    userBalance.CurrentBalance += transaction.Amount;
                    db.Entry(userBalance).State = EntityState.Modified;
                }
                db.SaveChanges();
            }

            Transaction entry = new Transaction()
            {
                TransactionTypeId = transaction.TransactionTypeId,
                Amount = transaction.Amount,
                UserId = transaction.UserId,
                TransactionDateTime = transaction.TransactionDateTime,
                SourceLedger = 1,
                DestinationLedger = 2
            };

            db.Transactions.Add(entry);
            db.SaveChanges();

            db.PendingTransactions.Remove(transaction);
            db.SaveChanges();


            return RedirectToAction("ViewPendingTransactions");
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