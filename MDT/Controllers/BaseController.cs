﻿using MDT.Filters;
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
                     .Include(u => u.Balances)
                     .Include(u => u.Balances.Select(b => b.Ledger))
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

            vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectId == group.GroupId && new List<int>() { 1, 5, 6 }.Contains(d.ObjectTypeId)).ToList());
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
                                .Include(d => d.DrawEntries)
                                .Include(d => d.DrawEntries.Select(e => e.User))
                                .Include(d => d.DrawResults)
                                .Include(d => d.DrawResults.Select(r => r.DrawEntry))
                                .Include(d => d.DrawResults.Select(r => r.DrawEntry.User))
                                .FirstOrDefault();

            DrawVM vm = null;
            if (draw != null)
            {

                if (draw.StartDateTime != null && draw.EndDateTime < DateTime.Now && draw.Results == null)
                {
                    if (draw.DrawType.IsInternal)
                    {
                        if (draw.DrawEntries.Any())
                        {
                            DetermineResults(draw);
                        }
                    }
                    else
                    {
                        draw.Results = "Pending";
                        db.Entry(draw).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                vm = new DrawVM(draw);
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

            if (d == null)
            {
                TempData["Error"] = "Operation Unsuccessful - Invalid Draw";
                return false;
            }
            if (dt == null || dt.GroupId != group.GroupId)
            {
                TempData["Error"] = "Operation Unsuccessful - Invalid Draw Type";
                return false;
            }
            if (d.StartDateTime == null)
            {
                TempData["Error"] = "Operation Unsuccessful - Drawing not started";
                return false;
            }
            if (d.EndDateTime < DateTime.Now)
            {
                TempData["Error"] = "Operation Unsuccessful - Drawing has ended.";
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

        protected bool RemoveDrawEntries(List<int> ids, bool isAdmin)
        {
            List<string> Results = new List<string>();
            foreach (int id in ids)
            {
                DrawEntry entry = db.DrawEntries.Find(id);

                if (entry == null)
                {
                    TempData["Error"] = "Operation Unsuccessful - Invalid Draw";
                    continue;
                }

                Draw d = db.Draws.Find(entry.DrawId);
                if (d == null)
                {
                    TempData["Error"] = "Operation Unsuccessful - Invalid Draw Type";
                    continue;
                }
                if (d.StartDateTime == null)
                {
                    TempData["Error"] = "Operation Unsuccessful - Drawing not started";
                    continue;
                }
                if (d.EndDateTime < DateTime.Now)
                {
                    TempData["Error"] = "Operation Unsuccessful - Drawing has ended.";
                    continue;
                }

                if (isAdmin || !entry.Draw.DrawType.RefundConfirmationRequired)
                {
                    if (CreateTransaction(entry.UserId, 7, entry.Draw.DrawType.EntryCost, entry.Draw.DrawType.LedgerId, false, group.AccountBalanceLedgerId, true, entry.DrawId))
                    {
                        Results.Add($"Removed entry {entry.EntryCode}, refunded {entry.Draw.DrawType.EntryCost}.");
                        db.Entry(entry).State = EntityState.Deleted;
                    }
                    else
                    {
                        Results.Add($"Failed to remove entry {entry.EntryCode}: {TempData["Error"]}.");
                        TempData.Remove("Error");
                    }
                }
                else
                {
                    entry.PendingRemoval = true;
                    Results.Add($"Requested removal for entry {entry.EntryCode}.");
                    db.Entry(entry).State = EntityState.Modified;

                    List<string> admins = db.GroupUsers.Where(gu => gu.GroupId == 0 && gu.IsAdmin).Select(gu => gu.User).ToList().Select(u => $"{u.EmailAddress}\t{u.UserName}").ToList();
                    Dictionary<string, string> variables = new Dictionary<string, string>()
                    {
                        { "[[ConfirmUrl]]", "Draw/ViewDraw/" + entry.DrawId}
                    };
                    
                    WebManager.SendTemplateEmail(admins, 14, variables);
                }
            }

            db.SaveChanges();
            TempData["Results"] = string.Join("<br />", Results);
            return true;
        }

        private void DetermineResults(Draw draw, int drawNumber = 1)
        {
            int toDraw = draw.DrawOption.EntriesToDraw;

            List<DrawEntry> undrawn = draw.DrawEntries.ToList();
            List<DrawEntry> drawn = new List<DrawEntry>();
            List<DrawResult> results = new List<DrawResult>();
            if (draw.DrawType.NumberOfDraws > 1 && drawNumber < draw.DrawType.NumberOfDraws)
            {
                toDraw = undrawn.Count / 2;
            }

            string res = "";
            for (int i = 1; i <= toDraw; i++)
            {
                int winIndex = WebManager.RandomNumber(undrawn.Count);
                DrawEntry winner = undrawn[winIndex];
                drawn.Add(winner);
                DrawResult result = new DrawResult()
                {
                    DrawId = draw.DrawId,
                    DrawCount = (draw.DrawType.NumberOfDraws > 1 ? toDraw + 1 - i : i),
                    EntryId = winner.EntryId,
                    DrawnDateTime = DateTime.Now,
                };
                res += $"{(res.Length > 0 ? ", " : "")}{winner.EntryCode}";
                results.Add(result);
                if (draw.DrawOption.RemoveDrawnEntries)
                {
                    undrawn.Remove(winner);
                }

                if (draw.DrawOption.RemoveDrawnUsers)
                {
                    undrawn.RemoveAll(e => e.UserId == winner.UserId);
                }

                if (!undrawn.Any())
                {
                    res += $"{(res.Length > 0 ? ", " : "")} No entries left to draw from";
                    break;
                }
            }

            db.DrawResults.AddRange(results);
            draw.Results = res;

            if (drawNumber < draw.DrawType.NumberOfDraws)
            {
                Draw nextDraw = new Draw()
                {
                    DrawTypeId = draw.DrawTypeId,
                    Title = draw.Title,
                    StartDateTime = draw.StartDateTime,
                    EndDateTime = draw.EndDateTime,
                    DrawCode = WebManager.GetUniqueKey(7)
                };

                nextDraw.DrawOption = new DrawOption()
                {
                    MaxEntriesPerUser = draw.DrawOption.MaxEntriesPerUser,
                    EntriesToDraw = draw.DrawOption.EntriesToDraw,
                    RemoveDrawnEntries = draw.DrawOption.RemoveDrawnEntries,
                    RemoveDrawnUsers = draw.DrawOption.RemoveDrawnUsers,
                    PassDrawnToNext = draw.DrawOption.PassDrawnToNext
                };


                if (draw.DrawOption.PassDrawnToNext)
                {
                    nextDraw.DrawEntries = drawn.Select(e => new DrawEntry()
                    {
                        EntryCode = e.EntryCode,
                        UserId = e.UserId
                    }).ToList();

                }
                else
                {
                    nextDraw.DrawEntries = undrawn.Select(e => new DrawEntry()
                    {
                        EntryCode = e.EntryCode,
                        UserId = e.UserId
                    }).ToList();
                }

                draw.DrawOption.NextDraw = nextDraw;
            }
            db.Entry(draw).State = EntityState.Modified;
            db.SaveChanges();

            if (draw.DrawOption.NextDraw != null)
            {
                DetermineResults(draw.DrawOption.NextDraw, drawNumber + 1);
            }
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