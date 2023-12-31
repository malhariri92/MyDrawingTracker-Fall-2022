﻿using MDT.Models.DTO;
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
                switch (permission?.ToLower())
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

        internal static int RandomNumber(int max)
        {
            byte[] b = new byte[4];
            RNG.GetBytes(b);
            decimal d = BitConverter.ToUInt32(b, 0) / (decimal)UInt32.MaxValue;
            return (int)(max * d);
        }
    }
}