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
                     .Where(dt => ids.Contains(dt.DrawTypeId) && dt.GroupDrawTypes.Any(gdt => gdt.GroupId == group.GroupId))
                     .Include(dt => dt.NumberSets)
                     .Include(dt => dt.Draws)
                     .Include(dt => dt.Schedules)
                     .Include(dt => dt.UserDrawTypeOptions)
                     .Include(dt => dt.UserDrawTypeOptions.Select(udto => udto.User))
                     .ToList();
        }

        protected DrawType GetDrawType(int id)
        {
            return (GetDrawTypes(new List<int>() { id })).FirstOrDefault();
        }

        protected List<User> GetUsers(List<int> ids)
        {
            return db.Users
                     .Where(u => ids.Contains(u.UserId))
                     .Include(u => u.CurrentGroupId)
                     .Include(u => u.GroupUsers)
                     .Include(u => u.UserDrawTypeOptions)
                     .Include(u => u.UserDrawTypeOptions.Select(udto => udto.DrawType))
                     .Include(u => u.DrawEntries)
                     .Include(u => u.DrawEntries.Select(de => de.Draw))
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
                                            .Include(dt => dt.Draws)
                                            .Include(dt => dt.Draws.Select(d => d.DrawType))
                                            .Include(dt => dt.Draws.Select(d => d.DrawOption))
                                            .FirstOrDefault();

            DrawTypeVM vm =  new DrawTypeVM(drawType);

            if (drawType != null)
            {
                vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectTypeId == 2 && d.ObjectId == drawType.DrawTypeId).ToList());
            }

            return vm;
        }

        protected DrawVM GetDrawVM(int id)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == id && d.DrawType.GroupDrawTypes.Any(gdt => gdt.GroupId == group.GroupId))
                                .Include(d => d.DrawType)
                                .Include(d => d.DrawOption)
                                .FirstOrDefault();
            
            DrawVM vm = new DrawVM(draw);
            if (draw != null)
            {
                vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectTypeId == 3 && d.ObjectId == draw.DrawId).ToList());
            }

            return vm;
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