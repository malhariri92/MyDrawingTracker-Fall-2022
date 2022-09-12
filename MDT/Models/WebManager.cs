using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace MDT.Models
{
    public static class WebManager
    {
        internal static string GenerateUserKey(UserPasswordResetSetupVM vm)
        {
            using (var db = new DbEntities())
            {
                User user = db.Users.Find(vm.UserId);
                if (user != null && user.EmailAddress.Equals(vm.UserEmail) && user.IsActive)
                {
                    string key = RandomString(25);
                    user.ResetKey = key;
                    user.ResetKeyExpires = DateTime.Now.AddMinutes(60);

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    return key;
                }
            }
            return null;
        }

        internal static UserVM GetUserVMByEmail(string email)
        {
            using (var db = new DbEntities())
            {
                User player = db.Users.Where(p => p.EmailAddress.Equals(email))
                                          .Include(p => p.GroupUsers)
                                          .Include(p => p.GroupUsers.Select(pg => pg.Group))
                                          .Include(p => p.Entries)
                                          .Include(p => p.Entries.Select(e => e.Draw))
                                          .Include(p => p.Transactions)
                                          .FirstOrDefault();
                return player == null ? null : new UserVM(player);

            }
        }

        internal static bool CheckCurrentHash(int userId, string str)
        {
            using (var db = new DbEntities())
            {
                string hash = db.Users.Find(userId)?.Hash;
                if (hash == null)
                {
                    return false;
                }

                return PasswordManager.TestHashMatch(str, hash);
            }

        }

        internal static UserVM GetUserVM(int userId)
        {
            using (var db = new DbEntities())
            {
                return new UserVM(db.Users.Where(u => u.UserId == userId).Include(u => u.GroupUsers).FirstOrDefault());
                
            }

        }
        internal static GroupVM GetGroupVM(int groupId)
        {
            using (var db = new DbEntities())
            {                
                return new GroupVM(db.Groups.Where(g => g.GroupId == groupId).Include(u => u.GroupUsers).FirstOrDefault());
            }
        }

        internal static bool IsGroupAdmin(int groupId, int userId)
        {
            using (var db = new DbEntities())
            {
                return db.GroupUsers.Find(groupId, userId)?.IsAdmin ?? false;
            }
        }

        internal static bool IsGroupMember(int groupId, int userId)
        {
            using (var db = new DbEntities())
            {
                return db.GroupUsers.Find(groupId, userId) != null;
            }
        }


        internal static string RandomString(int l)
        {
            Random r = new Random();
            string rand = "";
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
           
            for (int i = 0; i < l; i++)
            {
                rand += $"{chars[r.Next(chars.Length)]}";
            }

            return rand;
        }

        
    }
}