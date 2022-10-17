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

        protected List<Group> GetGroups(List<int> ids)
        {
            return db.Groups.Where(g => ids.Contains(g.GroupId))
                            .Include(g => g.GroupUsers)
                            .Include(g => g.GroupUsers.Select(gu => gu.User))
                            .ToList();
        }

        protected Group GetGroup(int id)
        {
            return (GetGroups(new List<int>() { id })).FirstOrDefault();
        }

        protected List<Draw> GetDraws(List<int> ids)
        {
            return db.Draws
                     .Where(g => ids.Contains(g.DrawId))
                     .Include(g => g.DrawType)
                     .Include(g => g.DrawEntries)
                     .Include(g => g.DrawEntries.Select(e => e.User))
                     .ToList();
        }

        protected Draw GetDraw(int id)
        {
            return (GetDraws(new List<int>() { id })).FirstOrDefault();
        }

        protected List<DrawType> GetDrawTypes(List<int> ids)
        {
            return db.DrawTypes
                     .Where(g => ids.Contains(g.DrawTypeId))
                     .Include(g => g.NumberSets)
                     .Include(g => g.Draws)
                     .Include(g => g.Schedules)
                     .Include(g => g.UserDrawTypeOptions)
                     .Include(g => g.UserDrawTypeOptions.Select(pg => pg.User))
                     .ToList();
        }

        protected DrawType GetDrawType(int id)
        {
            return (GetDrawTypes(new List<int>() { id })).FirstOrDefault();
        }

        protected List<User> GetUsers(List<int> ids)
        {
            return db.Users
                     .Where(g => ids.Contains(g.UserId))
                     .Include(g => g.CurrentGroupId)
                     .Include(g => g.GroupUsers)
                     .Include(g => g.UserDrawTypeOptions)
                     .Include(g => g.UserDrawTypeOptions.Select(pg => pg.DrawType))
                     .Include(g => g.DrawEntries)
                     .Include(g => g.DrawEntries.Select(e => e.Draw))
                     .ToList();
        }

        protected User GetUser(int id)
        {
            return (GetUsers(new List<int>() { id })).FirstOrDefault();
        }

        protected List<Transaction> GetTransactions(List<int> ids)
        {
            return db.Transactions
                     .Where(t => ids.Contains(t.TransactionId))
                     .Include(t => t.TransactionType)
                     .Include(t => t.User)
                     .Include(t => t.Draw)
                     .Include(t => t.Draw.DrawType)
                     .Include(t => t.Draw.DrawEntries)
                     .Include(t => t.SourceLedger)
                     .Include(t => t.DestinationLedger)
                     .Include(t => t.Draw.DrawEntries.Select(e => e.User))
                     .ToList();
        }

        protected Transaction GetTransaction(int id)
        {
            return (GetTransactions(new List<int>() { id })).FirstOrDefault();
        }

       
        protected List<DdlItem> GetDdl(DbSet<TransactionType> table)
        {
            return table.Where(t => t.IsActive)
                        .OrderBy(t => t.SortOrder)
                        .ToList()
                        .Select(i => new DdlItem(i.TransactionTypeId, i.TypeName))
                        .ToList();
        }

        protected List<DdlItem> GetDdl(DbSet<GroupUser> table)
        {
            return table.Where(i => i.GroupId == group.GroupId)
                        .ToList()
                        .Select(i => new DdlItem(i.UserId, i.User.UserName))
                        .ToList();
        }

        protected List<DdlItem> GetDdl(DbSet<DrawType> table)
        {
            return table.Where(i => i.GroupDrawTypes.Any(gdt => gdt.GroupId == group.GroupId))
                        .ToList()
                        .Select(i => new DdlItem(i.DrawTypeId, i.DrawTypeName)).ToList();
        }

        protected List<DdlItem> GetDdl(DbSet<Draw> table, int typeId = 0)
        {
            return table.Where(i => (typeId == 0 || i.DrawTypeId == typeId) && i.DrawType.GroupDrawTypes.Any(gdt => gdt.GroupId == group.GroupId))
                        .ToList()
                        .Select(i => new DdlItem(i.DrawId, $"{i.EndDateTime:yyyy-MM-dd HH:mm} ({i.DrawType.DrawTypeName})"))
                        .ToList();
        }

        protected DrawTypeVM GetDrawTypeVM(int id)
        {
            DrawType drawType = db.DrawTypes.Where(dt => dt.DrawTypeId == id && dt.GroupDrawTypes.Any(gdt => gdt.GroupId == group.GroupId))
                                            .Include(dt => dt.UserDrawTypeOptions)
                                            .Include(dt => dt.UserDrawTypeOptions.Select(udto => udto.User))
                                            .Include(dt => dt.Schedules)
                                            .FirstOrDefault();

            return new DrawTypeVM(drawType);
        }

        protected DrawVM GetDrawVM(int id)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == id && d.DrawType.GroupDrawTypes.Any(gdt => gdt.GroupId == group.GroupId))
                                .Include(d => d.DrawType)
                                .Include(d => d.DrawOption)
                                .FirstOrDefault();

            return new DrawVM(draw);
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