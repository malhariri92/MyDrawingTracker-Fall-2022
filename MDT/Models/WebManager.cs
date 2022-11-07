using MDT.Models.DTO;
using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Web.Helpers;

namespace MDT.Models
{
    public static class WebManager
    {
        private static RandomNumberGenerator RNG = RandomNumberGenerator.Create();
        internal static string GenerateUserKey(UserPasswordResetSetupVM vm)
        {
            using (var db = new DbEntities())
            {
                User user = db.Users.Find(vm.UserId);
                if (user != null && user.EmailAddress.Equals(vm.UserEmail, StringComparison.CurrentCultureIgnoreCase) && user.IsActive)
                {
                    string key = GetUniqueKey(25);
                    user.ResetKey = key;
                    user.ResetKeyExpires = DateTime.Now.AddMinutes(60);
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    return key;
                }
            }
            return null;
        }

        internal static UserDTO GetUserDTOByEmail(string email)
        {
            email = email?.ToLower();
            using (var db = new DbEntities())
            {
                User user = db.Users.Where(u => u.EmailAddress.Equals(email)).FirstOrDefault();
                return user == null ? null : new UserDTO(db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId && gu.UserId == user.UserId).Include(gu => gu.User).FirstOrDefault());
            }
        }

        internal static UserDTO GetUserDTO(int userId)
        {
            using (var db = new DbEntities())
            {
                User user = db.Users.Find(userId);
                return user == null ? null : new UserDTO(db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId && gu.UserId == userId).Include(gu => gu.User).FirstOrDefault());

            }

        }

        internal static GroupDTO GetGroupDTO(int groupId)
        {
            using (var db = new DbEntities())
            {
                return new GroupDTO(db.Groups.Where(g => g.GroupId == groupId)
                                             .Include(g => g.GroupUsers)
                                             .FirstOrDefault());
            }
        }

        public static bool IsGroupAdmin(int groupId, int userId)
        {
            using (var db = new DbEntities())
            {
                return db.GroupUsers.Find(groupId, userId)?.IsAdmin ?? false;
            }
        }

        public static bool HasPermission(int groupId, int userId, string permission)
        {
            using (var db = new DbEntities())
            {
                GroupUser gu = db.GroupUsers.Find(groupId, userId);
                if (gu?.IsAdmin ?? false)
                {
                    return true;
                }
                switch (permission.ToLower())
                {
                    case "users": return gu?.CanManageUsers ?? false;
                    case "drawtypes": return gu?.CanManageDrawTypes ?? false;
                    case "drawings": return gu?.CanManageDrawings ?? false;
                    case "transactions": return gu?.CanManageTransactions ?? false;
                    default: return false;
                }

            }
        }

        public static bool SendTemplateEmail(string recipient, int templateId, Dictionary<string, string> variables)
        {
            return SendTemplateEmail(new List<string> { recipient }, templateId, variables);
        }

        public static bool SendTemplateEmail(List<string> recipients, int templateId, Dictionary<string, string> variables)
        {
            if (recipients == null || !recipients.Any())
            {
                throw new ArgumentException($"Cannot send mail without recipients");
            }

            using (var db = new DbEntities())
            {
                EmailTemplate template = db.EmailTemplates.Find(templateId);
                if (template == null)
                {
                    throw new ArgumentException($"Template for id: {templateId} not found");
                }
                if (!variables.ContainsKey("[[TemplateName]]"))
                {
                    variables.Add("[[TemplateName]]", template.TemplateName);
                }
                EmailMessage email = new EmailMessage();
                email.AddTo(recipients);
                email.SetSubject(template.SubjectLine);
                email.SetTemplateBody(template.FileName, variables);
                SentEmail msg = new SentEmail()
                {
                    TemplateId = templateId,
                    Recipients = string.Join(";", recipients),
                    VariablesJSON = Newtonsoft.Json.JsonConvert.SerializeObject(variables)
                };

                if (email.SendMessage())
                {
                    msg.SentOn = DateTime.Now;
                }

                db.Entry(msg).State = EntityState.Added;
                db.SaveChanges();

                return msg.SentOn != null;

            }
        }

        internal static string GetUniqueKey(int l)
        {
            using (var db = new DbEntities())
            {
                string key = RandomString(l);
                UniqueKey uk = db.UniqueKeys.Find(l, key);

                while (uk != null)
                {
                    key = RandomString(l);
                    uk = db.UniqueKeys.Find(l, key);
                }

                uk = new UniqueKey()
                {
                    KeyLength = l,
                    KeyValue = key
                };

                db.Entry(uk).State = EntityState.Added;
                db.SaveChanges();

                return key;
            }
        }

        public static List<int> OnePrizePerEntry(int DrawId)
        {
            // Open the database.
            using (var db = new DbEntities())
            {
                // Get the associated draw from the database.
                Draw draw = db.Draws.Where(d => d.DrawId == DrawId).FirstOrDefault();
            
                // If this draw is not null, its DrawEntries list is not null, and it has draws of some kind,
                // initiate the draw.
                if (draw != null 
                    && draw.DrawEntries != null
                    && draw.DrawEntries.Count != 0 
                    && draw.DrawOption != null)
                {
                    // List of all entries. Entries will be removed upon being drawn.
                    List<DrawEntry> listOfEntries = draw.DrawEntries.ToList();

                    // List of DrawEntry objects that have been chose by the RNG algorithm.
                    List<int> listOfWinners = new List<int>();

                    // Number of total prizes. If the number of entries is actually smaller than the number of Entries that are
                    // supposed to be drawn, the prizeCount will be updated to reflect this.
                    int prizeCount = Math.Min(draw.DrawOption.EntriesToDraw, listOfEntries.Count);

                    Random r = new Random();

                    for (int i = 0; i < prizeCount; i++)
                    {
                        // Position of the drawn entry.
                        int drawnPos = r.Next(0, listOfEntries.Count);
                        
                        // Add the drawn DrawEntry object to listOfWinners as a new DrawEntryDTO object.
                        listOfWinners.Add(listOfEntries[drawnPos].EntryId);

                        // Create the DrawResult object for the draw.
                        DrawResult drawResult = new DrawResult()
                        {
                            DrawnDateTime = DateTime.Now,
                            DrawCount = i + 1,
                            DrawId = draw.DrawId,
                            EntryId = listOfWinners[i],

                        };        

                        // Update the state of the drawResult object.
                        db.Entry(drawResult).State = EntityState.Added;

                        // Remove the drawn entry.
                        listOfEntries.RemoveAt(drawnPos);
                    }

                    // Create a comma-separate list of the drawn entry IDs.
                    draw.Results = String.Join(",", listOfWinners);

                    // Save changes in the database.
                    db.SaveChanges();

                    // Return the list of winners.
                    return listOfWinners;
                }
            }
            
            // Return nothing; an error has probably occurred.
            return null;
        }
        
        internal static string RandomString(int l)
        {
            string rand = "";
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (int i = 0; i < l; i++)
            {
                int index = RandomNumber(chars.Length);

                char c = chars[index];

                rand += $"{c}";
            }

            return rand;
        }

        public static List<DrawEntry> EliminateEntries(Draw draw)
        {
            List<DrawEntry> pool = draw.DrawEntries.ToList();
            return DoEliminateEntries(pool, new List<DrawEntry>(), draw.DrawOption);
        }

        private static List<DrawEntry> DoEliminateEntries(List<DrawEntry> pool, List<DrawEntry> result, DrawOption options)
        {
            if (pool.Count == 0)
            {
                result.Reverse();
                return result;
            }
            else if (pool.Count == 1)
            {
                result.Add(pool[0]);
                result.Reverse();
                return result;
            }
            Random random = new Random();
            List<DrawEntry> nextPool = new List<DrawEntry>();
            for (int i = 0; i < pool.Count / 2; ++i)
            {
                int index = random.Next(0, pool.Count);
                nextPool.Add(pool[index]);

                pool.RemoveAt(index);
            }

            if (options.PassDrawnToNext)
            {
                result.AddRange(pool);
                return DoEliminateEntries(nextPool, result, options);
            }

            result.AddRange(nextPool);
            return DoEliminateEntries(pool, result, options);
        }
    

        internal static int RandomNumber(int max)
        {
            byte[] b = new byte[4];
            RNG.GetBytes(b);
            decimal d = BitConverter.ToUInt32(b, 0) / (decimal)UInt32.MaxValue;
            return (int)(max * d);
        }
    }
}