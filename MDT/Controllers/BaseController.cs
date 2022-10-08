using MDT.Filters;
using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MDT.Controllers
{
    [LoginFilter]
    [SetupFilter]
    [VerifiedFilter]
    public class BaseController : Controller
    {
        protected DbEntities db = new DbEntities();
        protected UserDTO user;
        protected GroupDTO group;
        protected bool admin;

        public BaseController() : base()
        {

        }

        public void Setup()
        {
            user = (UserDTO)Session["User"];
            group = (GroupDTO)Session["Group"];
        }

        protected async Task<List<Group>> GetGroups(List<int> ids)
        {
            return await db.Groups.Where(g => ids.Contains(g.GroupId))
                                      .Include(g => g.GroupUsers)
                                      .Include(g => g.GroupUsers.Select(gu => gu.User))
                                      .ToListAsync();
        }

        protected async Task<Group> GetGroup(int id)
        {
            return (await GetGroups(new List<int>() { id })).FirstOrDefault();
        }

         protected async Task<List<Draw>> GetDraws(List<int> ids)
        {
            return await db.Draws
                     .Where(g => ids.Contains(g.DrawId))
                     .Include(g => g.DrawType)
                     .Include(g => g.DrawEntries)
                     .Include(g => g.DrawEntries.Select(e => e.User))
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
                     .Include(g => g.DrawEntries)
                     .Include(g => g.DrawEntries.Select(e => e.Draw))
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
                     .Include(g => g.Draw.DrawEntries)
                     .Include(g => g.Draw.DrawEntries.Select(e => e.User))
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

        protected List<DdlItem> GetDdl(DbSet<TransactionType> table) 
        { 
            return table.ToList().Select(i => new DdlItem(i.TransactionTypeId, i.TypeName)).ToList(); 
        }

        protected List<DdlItem> GetDdl(DbSet<GroupUser> table)
        {
            return table.ToList().Where(i => i.GroupId == group.GroupId).Select(i => new DdlItem(i.UserId, i.User.UserName)).ToList();
        }
    }
}