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
                        ViewBag.Message = $"{targetUser.User.UserName} has been removed";
                        db.Entry(targetUser).State = EntityState.Deleted;
                        db.SaveChanges();

                        User u = db.Users.Find(id);
                        List<GroupUser> groupUsers = db.GroupUsers.Where(gu => gu.UserId == id).ToList();
                        if (!groupUsers.Any(gu => gu.Group.IsActive))
                        {
                            if (!groupUsers.Any(gu => gu.GroupId == -1))
                            {
                                GroupUser groupUser = new GroupUser()
                                {
                                    GroupId = -1,
                                    UserId = id,
                                    IsAdmin = false,
                                    IsApproved = true,
                                    IsOwner = false,
                                    CanManageDrawings = false,
                                    CanManageDrawTypes = false,
                                    CanManageTransactions = false,
                                    CanManageUsers = false
                                };

                                db.Entry(groupUser).State = EntityState.Added;
                                u.CurrentGroupId = -1;
                            }
                        }
                        else
                        {
                            u.CurrentGroupId = groupUsers.Where(gu => gu.Group.IsActive).FirstOrDefault().GroupId;
                        }

                        db.Entry(u).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }


            if (Request.UrlReferrer.ToString().ToLower().Contains("/user/member"))
            {
                return RedirectToAction("Members", "Group");
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


            if (Request.UrlReferrer.ToString().ToLower().Contains("/user/member"))
            {
                TempData["Message"] = ViewBag.Message;
                TempData["Error"] = ViewBag.Error;
                return RedirectToAction("Member", "User", new { id = targetUser.UserId });
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
            List<string> Emails = vm.EmailAddress.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string email in Emails)
            {
                string address = email.Trim().ToLower();
                GroupInvite invite = db.GroupInvites.Find(group.GroupId, address);
                if (invite != null)
                {
                    ViewBag.Error += $"{invite.EmailAddress} has already been invited to this group.<br />";
                    continue;
                }

                GroupUser gu = db.GroupUsers.Where(u => u.GroupId == group.GroupId && u.User.EmailAddress.Equals(address, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (gu != null)
                {
                    ViewBag.Error += $"{gu.User.UserName} ({address}) is already a {(gu.IsApproved ? "" : "pending ")} member of this group.<br />";
                    continue;
                }

                UserDTO targetUser = WebManager.GetUserDTOByEmail(address);

                invite = new GroupInvite()
                {
                    GroupId = group.GroupId,
                    EmailAddress = address,
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


                if (WebManager.SendTemplateEmail(targetUser == null ? $"{address}" : $"{address}\t{targetUser.UserName}", targetUser == null ? 11 : 12, variables))
                {
                    ViewBag.Message += $"An invitation has been sent to {address}.<br />";
                }
                else
                {

                    ViewBag.Error += $"Error sending invitation to {address}. <br />";
                }
            }
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
            if(!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView(vm);
            }
            Group grp = new Group()
            {
                GroupName = vm.GroupName,
                IsActive = false,
                AccessCode = WebManager.GetUniqueKey(10),
                AccountBalanceLedger = new Ledger()
                {
                    LedgerName = "Account Balance",
                    Balance = 0.0m
                }
            };

            db.Groups.Add(grp);
            db.SaveChanges();

            grp.GroupUsers.Add(new GroupUser()
            {
                UserId = user.UserId,
                IsAdmin = true,
                IsApproved = true,
                IsOwner = true
            });
            

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
                ObjectId = grp.GroupId,
                SortOrder = 1,
                TextBody = vm.Reason,
                IsHTML = false
            };

            db.Descriptions.Add(desc);

            db.SaveChanges();

            return PartialView(vm);
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult Promote(int id)
        {
            GroupUser usr = db.GroupUsers.Find(group.GroupId, id);

            if (usr == null)
            {
                ViewBag.Error = "That user no longer exists!";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));
            }

            if (usr.IsAdmin || usr.IsOwner)
            {
                ViewBag.Error = $"{usr.User.UserName} is already an admin!";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));

            }

            if (!usr.IsApproved)
            {
                ViewBag.Error = $"{usr.User.UserName} has not yet been approved!";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));

            }

            usr.IsAdmin = true;
            db.Entry(usr).State = EntityState.Modified;
            db.SaveChanges();

            if (Request.UrlReferrer.ToString().ToLower().Contains("/user/member"))
            {
                return RedirectToAction("Member", "User", new { id = usr.UserId });
            }

            return PartialView("GroupMembers", GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult Demote(int id)
        {

            GroupUser usr = db.GroupUsers.Find(group.GroupId, id);

            if (usr == null)
            {
                ViewBag.Error = "That user no longer exists!";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));
            }

            if (usr.IsOwner)
            {
                ViewBag.Error = $"{usr.User.UserName} cannot be demoted";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));
            }

            if (!usr.IsAdmin)
            {
                ViewBag.Error = $"{usr.User.UserName} is not an admin";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));
            }

            if (!usr.IsApproved)
            {
                ViewBag.Error = $"{usr.User.UserName} has not yet been approved!";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));

            }

            usr.IsAdmin = false;
            db.Entry(usr).State = EntityState.Modified;
            db.SaveChanges();

            if (Request.UrlReferrer.ToString().ToLower().Contains("/user/member"))
            {
                return RedirectToAction("Member", "User", new { id = usr.UserId });
            }

            return PartialView("GroupMembers", GetGroupVM(group.GroupId));
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult Permissions(int id)
        {
            GroupUser gu = db.GroupUsers.Find(group.GroupId, id);
            if (gu == null)
            {
                ViewBag.Error = "Error, could not find this user in this group.";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));
            }

            UserPermissionVM vm = new UserPermissionVM(gu);
            vm.UserName = db.Users.Where(u => u.UserId == id).FirstOrDefault().UserName;
            return PartialView(vm);
        }

        [AdminFilter(Role = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Permissions(UserPermissionVM vm)
        {
            GroupUser gu = db.GroupUsers.Find(group.GroupId, vm.UserId);
            if (gu == null)
            {
                ViewBag.Error = "Error, could not find this user in this group.";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));
            }

            if (gu.IsAdmin || gu.IsOwner)
            {
                ViewBag.Error = "Cannot change permissions for this user";
                return PartialView("GroupMembers", GetGroupVM(group.GroupId));
            }

            gu.CanManageDrawings = vm.CanManageDrawings;
            gu.CanManageDrawTypes = vm.CanManageDrawTypes;
            gu.CanManageTransactions = vm.CanManageTransactions;
            gu.CanManageUsers = vm.CanManageUsers;
            db.Entry(gu).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.Message = $"Permissions updated for {gu.User.UserName}";
            return PartialView("GroupMembers", GetGroupVM(group.GroupId));
        }
    }
}