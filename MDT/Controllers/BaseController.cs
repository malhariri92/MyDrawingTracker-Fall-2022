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
                            .Include(g => g.GroupInvites)
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
                     .Where(dt => ids.Contains(dt.DrawTypeId) && dt.GroupId == group.GroupId)
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
                     .Include(u => u.Group)
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
            return table.Where(i => i.GroupId == group.GroupId)
                        .ToList()
                        .Select(i => new DdlItem(i.DrawTypeId, i.DrawTypeName)).ToList();
        }

        protected List<DdlItem> GetDdl(DbSet<Draw> table, int typeId = 0)
        {
            return table.Where(i => (typeId == 0 || i.DrawTypeId == typeId) && i.DrawType.GroupId == group.GroupId)
                        .ToList()
                        .Select(i => new DdlItem(i.DrawId, $"{i.EndDateTime:yyyy-MM-dd HH:mm} ({i.DrawType.DrawTypeName})"))
                        .ToList();
        }

        protected GroupVM GetGroupVM(int id)
        {
            GroupVM vm = new GroupVM(db.Groups.Where(g => id == g.GroupId)
                                            .Include(g => g.GroupUsers)
                                            .Include(g => g.GroupUsers.Select(gu => gu.User))
                                            .Include(g => g.GroupInvites)
                                            .FirstOrDefault());

            vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectId == group.GroupId && new List<int>(){ 1, 5, 6 }.Contains(d.ObjectTypeId)).ToList());
            return vm;
        }

        protected DrawTypeVM GetDrawTypeVM(int id)
        {
            DrawType drawType = db.DrawTypes.Where(dt => dt.DrawTypeId == id && dt.GroupId == group.GroupId)
                                            .Include(dt => dt.UserDrawTypeOptions)
                                            .Include(dt => dt.UserDrawTypeOptions.Select(udto => udto.User))
                                            .Include(dt => dt.Schedules)
                                            .Include(dt => dt.Draws)
                                            .Include(dt => dt.Draws.Select(d => d.DrawType))
                                            .Include(dt => dt.Draws.Select(d => d.DrawOption))
                                            .FirstOrDefault();

            DrawTypeVM vm = new DrawTypeVM(drawType);

            if (drawType != null)
            {
                vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectTypeId == 2 && d.ObjectId == drawType.DrawTypeId).ToList());
                vm.SetUserOptions(db.UserDrawTypeOptions.Where(d => d.DrawTypeId == drawType.DrawTypeId).Include(u => u.User).ToList());
            }

            return vm;
        }

        protected DrawVM GetDrawVM(int id)
        {
            Draw draw = db.Draws.Where(d => d.DrawId == id && d.DrawType.GroupId == group.GroupId)
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

        protected Balance GetUserBalance(int ledgerId, int userId)
        {
            Balance b = db.Balances.Find(ledgerId, userId);
            if (b == null)
            {
                b = new Balance()
                {
                    LedgerId = ledgerId,
                    UserId = userId,
                    CurrentBalance = 0.0m
                };

                db.Entry(b).State = EntityState.Added;
                db.SaveChanges();
            }

            return b;
        }

        protected UserDrawTypeOption GetUserDrawTypeOption(int drawtypeId, int userId)
        {
            UserDrawTypeOption o = db.UserDrawTypeOptions.Find(userId, drawtypeId);
            if (o == null)
            {
                o = new UserDrawTypeOption()
                {
                    DrawTypeId = drawtypeId,
                    UserId = userId,
                    PlayAll = false,
                    IsApproved = false,
                    MaxPlay = 0,
                    Priority = 1
                };

                db.Entry(o).State = EntityState.Added;
                db.SaveChanges();
            }

            return o;
        }

        protected bool BalanceAvailable(int ledgerId, int userId, decimal amount)
        {
            return GetUserBalance(ledgerId, userId).CurrentBalance >= amount;
        }

        protected bool UpdateBalance(int ledgerId, int userId, decimal amount)
        {
            Balance b = GetUserBalance(ledgerId, userId);
            decimal initialBalance = b.CurrentBalance;
            b.CurrentBalance += amount;
            db.Entry(b).State = EntityState.Modified;
            db.SaveChanges();

            return b.CurrentBalance == initialBalance + amount;
        }

        protected bool UpdateBalance(int ledgerId, decimal amount)
        {
            Ledger l = db.Ledgers.Find(ledgerId);
            decimal initialBalance = l.Balance;
            l.Balance += amount;
            db.Entry(l).State = EntityState.Modified;
            db.SaveChanges();

            return l.Balance == initialBalance + amount;
        }

        protected bool CreateTransaction(int userId, int typeId, decimal amount, int sourceId, bool userSource, int destinationId, bool userDestination, int? drawId = null)
        {
            if (!userSource || BalanceAvailable(sourceId, userId, amount))
            {
                Transaction t = new Transaction()
                {
                    UserId = userId,
                    GroupId = group.GroupId,
                    TransactionTypeId = typeId,
                    TransactionDateTime = DateTime.Now,
                    Amount = amount,
                    SourceLedger = sourceId,
                    DestinationLedger = destinationId,
                    DrawId = drawId
                };

                db.Entry(t).State = EntityState.Added;
                db.SaveChanges();
                if ((userSource ? UpdateBalance(sourceId, userId, -amount) : UpdateBalance(sourceId, -amount)) &&
                    (userDestination ? UpdateBalance(destinationId, userId, amount) : UpdateBalance(destinationId, amount)) &&
                    t.TransactionId > 0)
                {
                    return true;
                }
                else
                {
                    TempData["Error"] = "Operation Unsuccessful";
                    return false;

                }
            }

            TempData["Error"] = "Insufficient Balance";
            return false;
        }

        protected bool GetDrawEntries(int drawId, int userId, int ledgerId, int count)
        {
            Draw d = db.Draws.Find(drawId);
            DrawType dt = d?.DrawType;

            if (d == null || dt == null || dt.GroupId != group.GroupId)
            {
                TempData["Error"] = "Operation Unsuccessful";
                return false;
            }
            Balance b = GetUserBalance(ledgerId, userId);

            if (!BalanceAvailable(ledgerId, userId, count * dt.EntryCost))
            {
                TempData["Error"] = "Insufficient balance for request";
                return false;
            }

            int userEntryCount = d.DrawEntries.Where(e => e.UserId == userId).Count();
            if ((d.DrawOption?.MaxEntriesPerUser ?? dt.MaxEntriesPerUser) != 0 && userEntryCount + count > (d.DrawOption?.MaxEntriesPerUser ?? dt.MaxEntriesPerUser))
            {
                TempData["Error"] = "Request causes user entries to exceed maximum allowed entries.";
                return false;
            }
            List<DrawEntry> Entries = new List<DrawEntry>();
            for (int i = 0; i < count; i++)
            {
                DrawEntry entry = new DrawEntry()
                {
                    DrawId = d.DrawId,
                    UserId = userId,
                    EntryCode = WebManager.GetUniqueKey(6),
                    PendingRemoval = false
                };
                db.Entry(entry).State = EntityState.Added;
                Entries.Add(entry);
            }

            db.SaveChanges();
            CreateTransaction(userId, 6, dt.EntryCost * count, ledgerId, true, dt.LedgerId, false, d.DrawId);
            TempData["Entries"] = Entries;
            return true;
        }

        protected bool RemoveDrawEntries(List<int> entryIds, bool isAdmin)
        {
            List<string> Results = new List<string>();
            foreach(int id in entryIds)
            {
                DrawEntry entry = db.DrawEntries.Find(id);
                if (isAdmin || !entry.Draw.DrawOption.RefundConfirmationRequired)
                {
                    if (CreateTransaction(entry.UserId, 7, entry.Draw.DrawType.EntryCost, entry.Draw.DrawType.LedgerId, false, group.AccountBalanceLedgerId, true, entry.DrawId))
                    {
                        Results.Add($"Removed entry {entry.EntryCode}, refunded {entry.Draw.DrawType.EntryCost}");
                        db.Entry(entry).State = EntityState.Deleted;
                    }
                    else
                    {
                        Results.Add($"Failed to remove entry {entry.EntryCode}: {TempData["Error"]}");
                        TempData.Remove("Error");
                    }
                }
                else
                {
                    entry.PendingRemoval = true;
                    Results.Add($"Requested removal for entry {entry.EntryCode}");
                    db.Entry(entry).State = EntityState.Modified;
                }
            }
            
            db.SaveChanges();
            TempData["Results"] = Results;
            return true;
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