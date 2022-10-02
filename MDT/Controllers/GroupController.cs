using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MDT.ViewModels;
using MDT.Models;
using System.Data.Entity;
using MDT.Models.DTO;
using MDT.Filters;
using Newtonsoft.Json;

namespace MDT.Controllers
{
    public class GroupController : BaseController
    {

        public ActionResult Index()
        {
            GroupOptionsVM vm = new GroupOptionsVM(db.Groups.Where(g => group.GroupId == g.GroupId).FirstOrDefault());
            vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectId == group.GroupId && d.ObjectTypeId == 1).ToList());
        
                List<User> ml = db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId).Select(gu => gu.User).ToList();
                List<UserDTO> mld = new List<UserDTO>();
                foreach (User u in ml)
                {
                    if (user.UserId == u.UserId)
                    {
                        continue;
                    }
                    GroupUser gu = db.GroupUsers.Where(g => g.UserId == u.UserId && g.GroupId == u.CurrentGroupId).FirstOrDefault();

                    mld.Add(new UserDTO(u));
                }
                return View(mld);
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult Edit()
        {
            GroupOptionsVM vm = new GroupOptionsVM(db.Groups.Where(g => group.GroupId == g.GroupId).FirstOrDefault());
            vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectId == group.GroupId && d.ObjectTypeId == 1).ToList());
            return View(vm);
        }

        public ActionResult GroupIndex()
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];
            if (!WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                return RedirectToAction("Index", "User");
            }

            if (group.JoinConfirmationRequired)
            {
                List<User> users = db.Users.Where(u => u.CurrentGroupId == group.GroupId && !u.IsActive).ToList();
                ViewBag.users = users;
            }

            return View();
        }

        [HttpPost]
        [AdminFilter(Role = "Admin")]
        public ActionResult Reject(UserVM vm)
        {
            User user = db.Users.Find(vm.UserId);
            user.IsActive = false;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return View();
        }

        [HttpPost]
        [AdminFilter(Role = "Admin")]
        public ActionResult Approve(UserVM vm)
        {
            User user = db.Users.Find(vm.UserId);
            GroupDTO group = WebManager.GetGroupDTO(user.CurrentGroupId);

            user.IsActive = true;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[userName]]", user.UserName },
                    { "[[groupName]]", group.GroupName },
                    { "[[TemplateName]]", "New Group User Apprroved" },
                };

            EmailMessage email = new EmailMessage();
            email.AddTo(user.EmailAddress);
            email.SetSubject("You Request Has Been Approved");
            email.SetTemplateBody("UserApproved.html", variables);

            List<string> recipients = new List<string>();
            recipients.Add(user.EmailAddress);
            WebManager.SendTemplateEmail(
                recipients,
                9,
                variables
            );

            return View();
        }



        public ActionResult Index()
        {
            List<User> ml = db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId).Select(gu => gu.User).ToList();
            List<UserDTO> mld = new List<UserDTO>();
            foreach (User u in ml)
            {
                if (user.UserId == u.UserId)
                {
                    continue;
                }
                GroupUser gu = db.GroupUsers.Where(g => g.UserId == u.UserId && g.GroupId == u.CurrentGroupId).FirstOrDefault();

                mld.Add(new UserDTO(u));
            }
            return View(mld);
        }

        public ActionResult RemoveFromGroup(int uId, int guId)
        {
            try
                {
                    GroupUser gremoved = db.GroupUsers.Where(r => r.UserId == uId && r.GroupId == guId).First();
                    GroupUser gremoved2 = db.GroupUsers.Remove(gremoved);

                    db.Entry(gremoved2).State = EntityState.Deleted;
                    db.SaveChanges();
                }
                catch { }
            
            return RedirectToAction("Index", "Group");
        }

        public ActionResult InviteList()
        {
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                return View(getInvites(group.GroupId));
            }

            return RedirectToAction("Index", "Home");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GroupOptionsVM vm)
        {
            if (ModelState.IsValid)
            {
                Group grp = db.Groups.Find(group.GroupId);
                grp.GroupName = vm.GroupName;
                db.Entry(grp).State = EntityState.Modified;
                Description d1 = db.Descriptions.Find(1, group.GroupId, 1) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 1, IsNew = true };
                d1.Title = vm.Descriptions[0].Title;
                d1.TextBody = vm.Descriptions[0].TextBody;
                db.Entry(d1).State = d1.IsNew ? EntityState.Added : EntityState.Modified;
                Description d2 = db.Descriptions.Find(1, group.GroupId, 2) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 2, IsNew = true };
                d2.Title = vm.Descriptions[1].Title;
                d2.TextBody = vm.Descriptions[1].TextBody;
                db.Entry(d2).State = d2.IsNew ? EntityState.Added : EntityState.Modified;
                Description d3 = db.Descriptions.Find(1, group.GroupId, 3) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 3, IsNew = true };
                d3.Title = vm.Descriptions[2].Title;
                d3.TextBody = vm.Descriptions[2].TextBody;
                db.Entry(d3).State = d3.IsNew ? EntityState.Added : EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [AdminFilter(Role = "Admin")]
        public void JoinConfirmation(bool flag)
        {
            Group grp = db.Groups.Find(group.GroupId);

            grp.JoinConfirmationRequired = flag;
            db.Entry(grp).State = EntityState.Modified;
            db.SaveChanges();
        }


        public ActionResult SendInvite(GroupInvite grpInvite)
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                GroupInvite dBGroupInvite =
                db.GroupInvites.Where(gI => grpInvite.EmailAddress == gI.EmailAddress && group.GroupId == gI.GroupId).FirstOrDefault();

                User dbUser = db.Users.Where(u => u.EmailAddress == grpInvite.EmailAddress).FirstOrDefault();

                Models.Group dbGroup = db.Groups.Where(g => g.GroupId == group.GroupId).FirstOrDefault();

                if (dbUser != null)
                {
                    if (db.GroupUsers.Where(gU => gU.UserId == dbUser.UserId && gU.GroupId == dbGroup.GroupId).FirstOrDefault() != null)
                    {
                        return PartialView("ExistingInGroup", dbUser);
                    }
                }

                if (dBGroupInvite == null)
                {
                    grpInvite.LastInviteDate = DateTime.Now;
                    grpInvite.GroupId = group.GroupId;
                    grpInvite.InviteCount = 1;
                    db.Entry(grpInvite).State = EntityState.Added;
                    db.SaveChanges();
                }

                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[receiver]]", dbUser != null ? dbUser.UserName : "" },
                    { "[[sender]]", user.UserName },
                    { "[[groupName]]", group.GroupName },
                    { "[[accessCode]]", dbGroup != null ? dbGroup.AccessCode : "" },
                };

                List<string> recipients = new List<string>();
                recipients.Add(grpInvite.EmailAddress);

                WebManager.SendTemplateEmail(recipients, 6, variables);

                return PartialView(grpInvite);
            }

            return RedirectToAction("Index", "Home");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TriggerPasswordReset(string email)
        {

            UserDTO user = WebManager.GetUserDTOByEmail(email);
            UserPasswordResetSetupVM vm = new UserPasswordResetSetupVM();

            vm.UserId = user.UserId;
            vm.UserEmail = user.EmailAddress;

            string key = WebManager.GenerateUserKey(vm);
            if (key != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[key]]", key},
                    { "[[name]]", user.UserName },
                };

                EmailMessage em = new EmailMessage();
                em.AddTo(user.EmailAddress);
                em.SetSubject("Password Reset Request");
                em.SetTemplateBody("ForgotPass.html", variables);

                List<string> recipients = new List<string>();
                recipients.Add(user.EmailAddress);
                if (WebManager.SendTemplateEmail(
                    recipients,
                    1,
                    variables
                ))
                {
                    vm.Success = true;
                }
                else
                {
                    vm.Error = true;
                }


            }

            return RedirectToAction("Index", "Group");
        }

        public ActionResult SendReminder(GroupInvite grpInvite)
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                bool sent = sendReminder(grpInvite.EmailAddress, group.GroupId);
                Session["sent"] = sent;
                if (!sent)
                {
                    return PartialView("EmailFailed");
                }
                return PartialView(grpInvite);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult ConfirmDelete(GroupInvite grpInvite)
        {
            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                return PartialView("ConfirmDelete", grpInvite);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteInvite(GroupInvite grpInvite)
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                deleteInvitation(grpInvite.EmailAddress, group.GroupId);

                return PartialView(grpInvite);
            }
            return RedirectToAction("Index", "Home");
        }

        private List<GroupInvite> getInvites(int groupId)
        {
            return db.GroupInvites.Where(gI => gI.GroupId == group.GroupId).ToList();
        }

        private bool sendReminder(string EmailAddress, int GroupId)
        {
            GroupInvite reminding = db.GroupInvites.Where(gI => gI.EmailAddress == EmailAddress && gI.GroupId == GroupId).FirstOrDefault();

            if (reminding != null)
            {
                reminding.LastInviteDate = DateTime.Now;
                db.Entry(reminding).State = EntityState.Modified;
                db.SaveChanges();

                User dbUser = db.Users.Where(u => u.EmailAddress == EmailAddress).FirstOrDefault();
                Models.Group dbGroup = db.Groups.Where(g => g.GroupId == group.GroupId).FirstOrDefault();

                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[receiver]]", dbUser != null ? dbUser.UserName : ""},
                    { "[[sender]]", user.UserName },
                    { "[[groupName]]", group.GroupName },
                    { "[[accessCode]]", dbGroup != null ? dbGroup.AccessCode : "" },
                };

                List<string> recipients = new List<string>();
                recipients.Add(EmailAddress);

                WebManager.SendTemplateEmail(recipients, 6, variables);
            }
            return false;
        }

        private void deleteInvitation(string EmailAddress, int GroupId)
        {
            GroupInvite delete = db.GroupInvites
                .Where(
                    gI => gI.GroupId == GroupId && gI.EmailAddress == EmailAddress)
                .FirstOrDefault();
            if (delete != null)
            {
                db.Entry(delete).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }
    }
}