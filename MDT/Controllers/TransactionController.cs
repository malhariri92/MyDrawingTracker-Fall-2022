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

            else
            {
                var foundType = db.TransactionTypes.Where(t => t.TransactionTypeId == vm.TransactionTypeId).FirstOrDefault();
                if (foundType.IsDebit)
                {
                    var userBalance = db.Balances.Where(b => b.UserId == vm.UserId && b.LedgerId == 2).FirstOrDefault();
                    if (userBalance == null)
                    {
                        Balance bal = new Balance()
                        {
                            UserId = vm.UserId,
                            LedgerId = 2,
                            CurrentBalance = vm.Amount
                        };
                        db.Balances.Add(bal);
                    }

                    else
                    {
                        userBalance.CurrentBalance += vm.Amount;
                        db.Entry(userBalance).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }

                Transaction entry = new Transaction()
                {
                    TransactionTypeId = vm.TransactionTypeId,
                    Amount = vm.Amount,
                    UserId = vm.UserId,
                    TransactionDateTime = DateTime.Now,
                    SourceLedger = 1,
                    DestinationLedger = 2
                };

                db.Transactions.Add(entry);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }
        
        public ActionResult Index()
        {
                
            List<TransactionDTO> tld = new List<TransactionDTO>();
            using (var db = new DbEntities())
            {
                if (WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId))
                {
                    List<User> ml = db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId).Select(gu => gu.User).ToList();
                    foreach (User u in ml)
                    {
                        List<Transaction> transactions = db.Transactions.Where(t => t.UserId == u.UserId).ToList();
                        foreach(Transaction t in transactions)
                        {
                            tld.Add(new TransactionDTO(t));
                        }
                    }
                }
                else
                {
                    List<Transaction> transactions = db.Transactions.Where(t => t.UserId == user.UserId).ToList();
                    foreach(Transaction t in transactions)
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
                    TransactionDateTime = DateTime.Now,
                    SourceLedger = 1,
                    DestinationLedger = 2
                };

                db.PendingTransactions.Add(entry);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }
    }
}