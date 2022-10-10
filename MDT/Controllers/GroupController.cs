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
            return View(vm);
        }

        public ActionResult Members()
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
                return PartialView(mld);
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
            GroupUser targetUser = db.GroupUsers.Find(group.GroupId, vm.UserId);
            if (targetUser != null)
            {
                db.Entry(targetUser).State = EntityState.Deleted;
                db.SaveChanges();
            }

            return View();
        }

        [HttpPost]
        [AdminFilter(Role = "Admin")]
        public ActionResult Approve(UserVM vm)
        {
            GroupUser targetUser = db.GroupUsers.Find(group.GroupId, vm.UserId);

            if (targetUser != null)
            {
                targetUser.IsApproved = true;
                db.Entry(targetUser).State = EntityState.Modified;
                db.SaveChanges();

                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[UserName]]", targetUser.User.UserName },
                    { "[[GroupName]]", group.GroupName },
                };

                WebManager.SendTemplateEmail($"{targetUser.User.EmailAddress}\t{targetUser.User.UserName}", 9, variables);
            }
            return View();
        }


        public ActionResult RemoveFromGroup(int uId, int guId)
        {
            try
            {
                GroupUser targetUser = db.GroupUsers.Find(group.GroupId, uId);
                if (targetUser != null)
                {
                    db.Entry(targetUser).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            }
            catch { }

            return RedirectToAction("Index", "Group");
        }

        [AdminFilter(Role="Admin")]
        public ActionResult InviteList()
        {
           return View(db.GroupInvites.Where(gI => gI.GroupId == group.GroupId).ToList());          
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


        [AdminFilter(Role = "Admin")]
        public ActionResult SendInvite(GroupInvite grpInvite)
        {
            GroupInvite dBGroupInvite =
            db.GroupInvites.Where(gI => grpInvite.EmailAddress == gI.EmailAddress && group.GroupId == gI.GroupId).FirstOrDefault();

            User targetUser = db.Users.Where(u => u.EmailAddress == grpInvite.EmailAddress).FirstOrDefault();

            Models.Group dbGroup = db.Groups.Where(g => g.GroupId == group.GroupId).FirstOrDefault();

            if (targetUser != null)
            {
                if (db.GroupUsers.Where(gU => gU.UserId == targetUser.UserId && gU.GroupId == dbGroup.GroupId).FirstOrDefault() != null)
                {
                    return PartialView("ExistingInGroup", targetUser);
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
                    { "[[Name]]", targetUser?.UserName ?? "" },
                    { "[[AdminName]]", user.UserName },
                    { "[[GroupName]]", group.GroupName },
                    { "[[Code]]", group.AccessCode ?? "" },
                };


            if (targetUser == null)
            {
                WebManager.SendTemplateEmail($"{grpInvite.EmailAddress}", 6, variables);
            }
            else
            {

                WebManager.SendTemplateEmail($"{targetUser.EmailAddress}\t{targetUser.UserName}", 10, variables);
            }

            return PartialView(grpInvite);

        }

        [HttpPost]
        [AdminFilter(Role = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult TriggerPasswordReset(string email)
        {

            UserDTO targetUser = WebManager.GetUserDTOByEmail(email);
            UserPasswordResetSetupVM vm = new UserPasswordResetSetupVM();

            vm.UserId = targetUser.UserId;
            vm.UserEmail = targetUser.EmailAddress;

            string key = WebManager.GenerateUserKey(vm);
            if (key != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[AdminName]]", user.UserName},
                    { "[[GroupName]]", group.GroupName},
                    { "[[Key]]", key},
                    { "[[Name]]", targetUser.UserName },
                };

                if (WebManager.SendTemplateEmail($"{targetUser.EmailAddress}\t{targetUser.UserName}", 13, variables))
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

        [AdminFilter(Role = "Admin")]
        public ActionResult SendReminder(GroupInvite grpInvite)
        {
            bool sent = SendReminder(grpInvite.EmailAddress, group.GroupId);
            Session["sent"] = sent;
            if (!sent)
            {
                return PartialView("EmailFailed");
            }
            return PartialView(grpInvite);
        }

        [HttpPost]
        [AdminFilter(Role = "Admin")]
        public ActionResult ConfirmDelete(GroupInvite grpInvite)
        {
            return PartialView("ConfirmDelete", grpInvite);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteInvite(GroupInvite grpInvite)
        {

            deleteInvitation(grpInvite.EmailAddress, group.GroupId);

            return PartialView(grpInvite);
        }


        private bool SendReminder(string EmailAddress, int GroupId)
        {
            GroupInvite reminding = db.GroupInvites.Find(GroupId, EmailAddress);

            if (reminding != null)
            {
                reminding.LastInviteDate = DateTime.Now;
                db.Entry(reminding).State = EntityState.Modified;
                db.SaveChanges();

                User targetUser = db.Users.Where(u => u.EmailAddress == EmailAddress).FirstOrDefault();

                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[Name]]", targetUser?.UserName ?? "" },
                    { "[[AdminName]]", user.UserName },
                    { "[[GroupName]]", group.GroupName },
                    { "[[Code]]", group.AccessCode ?? "" },
                };

                if (targetUser == null)
                {
                    WebManager.SendTemplateEmail($"{reminding.EmailAddress}", 11, variables);
                }
                else
                {

                    WebManager.SendTemplateEmail($"{targetUser.EmailAddress}\t{targetUser.UserName}", 12, variables);
                }
            }
            return false;
        }

        private void deleteInvitation(string EmailAddress, int GroupId)
        {
            GroupInvite delete = db.GroupInvites.Find(GroupId,EmailAddress);
            if (delete != null)
            {
                db.Entry(delete).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }
    }
}