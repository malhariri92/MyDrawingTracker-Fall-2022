using MDT.Filters;
using MDT.Models;
using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MDT.Controllers
{
    [LoginFilter]
    [SetupFilter]
    public class BaseController : Controller
    {
        protected DbEntities db = new DbEntities();
        protected UserVM user;
        protected GroupVM group;
        protected bool admin;

        public BaseController() : base()
        {

        }

        public void Setup()
        {
            user = (UserVM)Session["User"];
            group = (GroupVM)Session["Group"];
            admin = (bool)Session["IsAdmin"];
        }

        protected async Task<List<Draw>> GetDraws(List<int> ids)
        {
            return await db.Draws
                     .Where(g => ids.Contains(g.DrawId))
                     .Include(g => g.DrawType)
                     .Include(g => g.Entries)
                     .Include(g => g.Entries.Select(e => e.User))
                     .ToListAsync();
        }

        protected async Task<Draw> GetDraw(int id)
        {
            return (await GetDraws(new List<int>() { id })).FirstOrDefault();
        }

        protected async Task<List<DrawType>> GetDrawTypes(List<int> ids)
        {
            return await db.DrawTypes
                     .Where(g => ids.Contains(g.DrawTypeId))
                     .Include(g => g.NumberSets)
                     .Include(g => g.Draws)
                     .Include(g => g.Schedules)
                     .Include(g => g.UserDrawTypeOptions)
                     .Include(g => g.UserDrawTypeOptions.Select(pg => pg.User))
                     .ToListAsync();
        }

        protected async Task<DrawType> GetDrawType(int id)
        {
            return (await GetDrawTypes(new List<int>() { id })).FirstOrDefault();
        }

        protected async Task<List<User>> GetUsers(List<int> ids)
        {
            return await db.Users
                     .Where(g => ids.Contains(g.UserId))
                     .Include(g => g.CurrentGroupId)
                     .Include(g => g.GroupUsers)
                     .Include(g => g.UserDrawTypeOptions)
                     .Include(g => g.UserDrawTypeOptions.Select(pg => pg.DrawType))
                     .Include(g => g.Entries)
                     .Include(g => g.Entries.Select(e => e.Draw))
                     .ToListAsync();
        }

        protected async Task<User> GetUser(int id)
        {
            return (await GetUsers(new List<int>() { id })).FirstOrDefault();
        }

        protected async Task<List<Transaction>> GetTransactions(List<int> ids)
        {
            return await db.Transactions
                     .Where(g => ids.Contains(g.TransactionId))
                     .Include(g => g.TransactionType)
                     .Include(g => g.User)
                     .Include(g => g.Draw)
                     .Include(g => g.Draw.DrawType)
                     .Include(g => g.Draw.Entries)
                     .Include(g => g.Draw.Entries.Select(e => e.User))
                     .ToListAsync();
        }

        protected async Task<Transaction> GetTransaction(int id)
        {
            return (await GetTransactions(new List<int>() { id })).FirstOrDefault();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}