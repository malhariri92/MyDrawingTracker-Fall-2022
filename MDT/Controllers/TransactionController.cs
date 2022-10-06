using MDT.Models;
using MDT.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MDT.Controllers
{
    public class TransactionController : BaseController
    {
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
    }
}