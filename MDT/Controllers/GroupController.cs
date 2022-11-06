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
            GroupVM vm = GetGroupVM(user.CurrentGroupId);
            return View(vm);
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult Edit()
        {
            GroupVM vm = GetGroupVM(user.CurrentGroupId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GroupVM vm)
        {
            if (ModelState.IsValid)
            {
                Group grp = db.Groups.Find(group.GroupId);
                grp.GroupName = vm.GroupName;
                db.Entry(grp).State = EntityState.Modified;
                Description d1 = db.Descriptions.Find(1, group.GroupId, 1) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 1, IsNew = true };
                d1.Title = vm.InfoDesc[0].Title;
                d1.TextBody = vm.InfoDesc[0].TextBody;
                db.Entry(d1).State = d1.IsNew ? EntityState.Added : EntityState.Modified;
                Description d2 = db.Descriptions.Find(1, group.GroupId, 2) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 2, IsNew = true };
                d2.Title = vm.InfoDesc[1].Title;
                d2.TextBody = vm.InfoDesc[1].TextBody;
                db.Entry(d2).State = d2.IsNew ? EntityState.Added : EntityState.Modified;
                Description d3 = db.Descriptions.Find(1, group.GroupId, 3) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 3, IsNew = true };
                d3.Title = vm.InfoDesc[2].Title;
                d3.TextBody = vm.InfoDesc[2].TextBody;
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

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult Members()
        {
            GroupVM vm = GetGroupVM(user.CurrentGroupId);
            return View(vm);
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult GroupMembers()
        {
            return PartialView(GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult GroupPending()
        {
            return PartialView(GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult GroupInvites()
        {
            return PartialView(GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult Remove(int id)
        {
            GroupUser targetUser = db.GroupUsers.Find(group.GroupId, id);

            if (targetUser != null)
            {
                if (!targetUser.IsApproved)
                {
                    ViewBag.Error = "Cannot remove a pending member";
                }
                else
                {
                    if (targetUser.IsOwner)
                    {
                        ViewBag.Error = "Cannot remove the group owner";
                    }
                    else
                    {
                        db.Entry(targetUser).State = EntityState.Deleted;
                        db.SaveChanges();
                        ViewBag.Message = $"{targetUser.User.UserName} has been removed";
                    }
                }
            }
            return PartialView("GroupMembers", GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult TriggerPasswordReset(int id)
        {
            GroupUser targetUser = db.GroupUsers.Find(group.GroupId, id);
            if (targetUser == null)
            {
                ViewBag.Error = $"Cannot trigger the password reset for a user that is not in this group.";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));
            }

            UserPasswordResetSetupVM vm = new UserPasswordResetSetupVM();

            vm.UserId = targetUser.UserId;
            vm.UserEmail = targetUser.User.EmailAddress;

            string key = WebManager.GenerateUserKey(vm);
            if (key != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[AdminName]]", user.UserName},
                    { "[[GroupName]]", group.GroupName},
                    { "[[Key]]", key},
                    { "[[Name]]", targetUser.User.UserName },
                };


                if (WebManager.SendTemplateEmail($"{targetUser.User.EmailAddress}\t{targetUser.User.UserName}", 13, variables))
                {
                    ViewBag.Message = $"Password reset email has been sent to {targetUser.User.EmailAddress}";
                }
                else
                {
                    ViewBag.Error = $"Error sending password reset email to {targetUser.User.EmailAddress}. Please try again.";
                }
            }

            return PartialView("GroupMembers", GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult Approve(int id)
        {
            GroupUser targetUser = db.GroupUsers.Find(group.GroupId, id);

            if (targetUser != null)
            {
                if (targetUser.IsApproved)
                {
                    ViewBag.Error = "Cannot approve an existing member";
                }
                else
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
                    ViewBag.Message = $"{targetUser.User.UserName} has been approved";
                    ViewBag.RefreshGroupMembers = true;
                }
            }
            return PartialView("GroupPending", GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult Reject(int id)
        {
            GroupUser targetUser = db.GroupUsers.Find(group.GroupId, id);
            if (targetUser != null)
            {
                if (targetUser.IsApproved)
                {
                    ViewBag.Error = "Cannot reject an existing member";
                }
                else
                {
                    string name = targetUser.User.UserName;
                    db.Entry(targetUser).State = EntityState.Deleted;
                    db.SaveChanges();
                    ViewBag.Message = $"{name} has been rejected";
                }
            }

            return PartialView("GroupPending", GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult NewInvite()
        {
            return PartialView();
        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewInvite(GroupInvite vm)
        {
            GroupInvite invite = db.GroupInvites.Find(group.GroupId, vm.EmailAddress);
            if (invite != null)
            {
                ViewBag.Error = $"{invite.EmailAddress} has already been invited to this group.";
                return PartialView("GroupInvites", GetGroupVM(group.GroupId));
            }

            GroupUser gu = db.GroupUsers.Where(u => u.GroupId == group.GroupId && u.User.EmailAddress.Equals(vm.EmailAddress, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (gu != null)
            {
                ViewBag.Error = $"{gu.User.UserName} is already a {(gu.IsApproved ? "" : "pending ")} member of this group.";
                return PartialView("GroupInvites", GetGroupVM(group.GroupId));
            }

            UserDTO targetUser = WebManager.GetUserDTOByEmail(vm.EmailAddress);

            invite = new GroupInvite()
            {
                GroupId = group.GroupId,
                EmailAddress = vm.EmailAddress,
                LastInviteDate = DateTime.Now,
                InviteCount = 1
            };


            db.Entry(invite).State = EntityState.Added;
            db.SaveChanges();

            Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[Name]]", targetUser?.UserName ?? "" },
                    { "[[AdminName]]", user.UserName },
                    { "[[GroupName]]", group.GroupName },
                    { "[[Code]]", group.AccessCode ?? "" },
                };


            if (WebManager.SendTemplateEmail(targetUser == null ? $"{invite.EmailAddress}" : $"{targetUser.EmailAddress}\t{targetUser.UserName}", targetUser == null ? 11 : 12, variables))
            {
                ViewBag.Message = $"Invitation reminder has been sent to {invite.EmailAddress}";
            }
            else
            {

                ViewBag.Error = $"Error sending invitation reminder to {invite.EmailAddress}. Please try again.";
            }

            ViewBag.Message = $"An invite has been sent to {vm.EmailAddress}.";
            return PartialView("GroupInvites", GetGroupVM(group.GroupId));

        }

        [AdminFilter(Role = "Admin", Permission = "Users")]
        public ActionResult SendReminder(string email)
        {
            GroupInvite invite = db.GroupInvites.Find(group.GroupId, email);
            if (invite == null)
            {
                ViewBag.Error = $"{invite.EmailAddress} has not been invited to this group yet.";
                return PartialView("GroupInvites", GetGroupVM(group.GroupId));
            }

            invite.LastInviteDate = DateTime.Now;
            db.Entry(invite).State = EntityState.Modified;
            db.SaveChanges();

            UserDTO targetUser = WebManager.GetUserDTOByEmail(email);

            Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[Name]]", targetUser?.UserName ?? "" },
                    { "[[AdminName]]", user.UserName },
                    { "[[GroupName]]", group.GroupName },
                    { "[[Code]]", group.AccessCode ?? "" },
                };

            if (WebManager.SendTemplateEmail(targetUser == null ? $"{invite.EmailAddress}" : $"{targetUser.EmailAddress}\t{targetUser.UserName}", targetUser == null ? 11 : 12, variables))
            {
                ViewBag.Message = $"Invitation reminder has been sent to {email}";
            }
            else
            {

                ViewBag.Error = $"Error sending invitation reminder to {email}. Please try again.";
            }

            return PartialView("GroupInvites", GetGroupVM(group.GroupId));
        }

        [ValidateAntiForgeryToken]
        public ActionResult DeleteInvite(string email)
        {

            GroupInvite invite = db.GroupInvites.Find(group.GroupId, email);
            if (invite == null)
            {
                ViewBag.Error = $"Cannot delete invite. {invite.EmailAddress} has not been invited to this group yet.";
                return PartialView("GroupInvites", GetGroupVM(group.GroupId));
            }

            else
            {
                db.Entry(invite).State = EntityState.Deleted;
                db.SaveChanges();

                ViewBag.Message = $"Invitation for {email} has been deleted and is no longer valid";
            }

            return PartialView("GroupInvites", GetGroupVM(group.GroupId));
        }

        public ActionResult CreateNew()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult CreateNew(NewGroupVM vm)
        {
            Group group = new Group()
            {
                GroupName = vm.GroupName,
                IsActive = false,
                AccessCode = WebManager.GetUniqueKey(10)
            };

            db.Groups.Add(group);
            db.SaveChanges();

            UserDTO user = (UserDTO)Session["User"];

            GroupUser groupUser = new GroupUser()
            {
                GroupId = group.GroupId,
                UserId = user.UserId,
                IsAdmin = true,
                IsApproved = true,
                IsOwner = true
            };
            db.GroupUsers.Add(groupUser);
            db.SaveChanges();

            Dictionary<string, string> variables = new Dictionary<string, string>()
            {
                { "[[GroupName]]", group.GroupName },
                { "[[Reason]]", vm.Reason }
            };
            List<string> recipients = db.GroupUsers.Where(gu => gu.GroupId == 0 && gu.IsAdmin).Select(gu => gu.User).ToList().Select(u => $"{u.EmailAddress}\t{u.UserName}").ToList();
            WebManager.SendTemplateEmail(recipients, 3, variables);

            Description desc = new Description()
            {
                ObjectTypeId = 5,
                ObjectId = group.GroupId,
                SortOrder = 1,
                TextBody = vm.Reason,
                IsHTML = false
            };

            db.Descriptions.Add(desc);
            db.SaveChanges();

            return PartialView(vm);
        }
    }
}