using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using MDT.Models;
using MDT.ViewModels;
using System.Net;
using MDT.Filters;
using System.Data.Entity;
using MDT.Models.DTO;

namespace MDT.Controllers
{
    public class UserController : BaseController
    {

        public ActionResult Index()
        {
            UserVM vm = new UserVM(GetUser(user.UserId), group.GroupId);
            ViewBag.Draws = db.Draws.Where(d => d.DrawType.GroupId == group.GroupId && d.StartDateTime != null && d.EndDateTime > DateTime.Now)
                                    .Include(d => d.DrawEntries)
                                    .Include(d => d.DrawType)
                                    .Include(d => d.DrawOption)
                                    .ToList()
                                    .Select(d => new DrawVM(d))
                                    .ToList();
            return View(vm);
        }



        private List<Draw> GetDraws()
        {
            return db.Draws
                .Where(d => d.DrawType.GroupId == group.GroupId && d.EndDateTime > DateTime.Now)
                .ToList();
        }


        public ActionResult ChangePass()
        {
            return PartialView(new UserPasswordChangeVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(UserPasswordChangeVM vm)
        {

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView(vm);
            }
            if (vm.CurrentPassword.Equals(vm.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "New password must be different from current password.");
                Response.StatusCode = 400;
                return PartialView(vm);
            }

            if (!CheckCurrentHash(user.UserId, vm.CurrentPassword))
            {
                Response.StatusCode = 400;
                ModelState.AddModelError("CurrentPassword", "Current password incorrect.");
                return PartialView(vm);
            }

            ModalMessageVM mm = new ModalMessageVM();
            if (PasswordManager.SetNewHash(user.UserId, vm.NewPassword))
            {
                mm.Header = "Password Changed";
                mm.Body = $"Your password has been updated. You'll use your new password the next time you sign in.";
                mm.RedirectButton = false;
            }
            else
            {
                mm.Header = "Password Not Changed";
                mm.Body = $"Something went wrong updating your password.Please try again.";
                mm.RedirectButton = false;
                mm.HtmlFooter = true;
                mm.Footer = "<div class=\"form-group\">" +
                            "<div class=\"col-xs-8 pull-right\">" +
                            "<button type=\"button\" class=\"btn btn-warning btn-xs btnModal\" data-action=\"@Url.Action(\"ChangePass\", \"User\", null)\">" +
                            "Try Again" +
                            "</button>" +
                            "</div>" +
                            "</div>";

            }
            return PartialView(vm);
        }

        public ActionResult Edit()
        {
            return PartialView(db.Users.Find(user.UserId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User u)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView(u);
            }

            User usr = db.Users.Find(user.UserId);
            usr.UserName = u.UserName;
            db.Entry(usr).State = EntityState.Modified;
            db.SaveChanges();

            UserVM vm = new UserVM(GetUser(user.UserId), group.GroupId);
            return PartialView("Details", vm);

        }

        public ActionResult Member(int id)
        {
            UserVM vm = new UserVM(GetUser(id), group.GroupId);
            return View(vm);
        }

        public ActionResult Join()
        {
            return PartialView();
        }

        [HttpPost]

        public ActionResult Join(JoinVM vm)
        {
            Group grp = db.Groups.Where(g => g.AccessCode.Equals(vm.AccessCode)).FirstOrDefault();
            if (grp == null)
            {
                ModelState.AddModelError("AccessCode", "Invalid Group Access Code.");
                Response.StatusCode = 400;
                return PartialView(vm);
            }

            if (grp.GroupUsers.Any(gu => gu.UserId == user.UserId))
            {
                ModelState.AddModelError("AccessCode", $"You are already a member of { grp.GroupName}");
                Response.StatusCode = 400;
                return PartialView(vm);
            }

            GroupUser grpUsr = new GroupUser()
            {
                GroupId = grp.GroupId,
                IsAdmin = false,
                IsApproved = !grp.JoinConfirmationRequired,
                IsOwner = false,
                UserId = user.UserId,
            };

            db.Entry(grpUsr).State = EntityState.Added;

            db.SaveChanges();

            ModalMessageVM mm = new ModalMessageVM()
            {
                Header = "Access Code Accepted",
                Body = grp.JoinConfirmationRequired ? $"You have been added to the pending users list for {grp.GroupName}." : $"You have joined {grp.GroupName}",
                RedirectButton = !grp.JoinConfirmationRequired,
                RedirectLink = Url.Action("ChangeGroup", "Home", new { groupId = grp.GroupId }),
                RedirectText = "Go to group"
            };

            return PartialView("ModalMessage", mm);
        }

        public ActionResult ChangeGroup(int id)
        {
            if (user.CurrentGroupId != id && db.GroupUsers.Find(id, user.UserId) != null)
            {
                GroupUser u = db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId && gu.UserId == user.UserId).Include(gu => gu.User).FirstOrDefault();
                u.User.CurrentGroupId = id;
                db.Entry(u.User).State = EntityState.Modified;
                db.SaveChanges();
                Session["User"] = new UserDTO(u);
                Session["Group"] = WebManager.GetGroupDTO(id);
            }

            return RedirectToAction("Index", "User", null);
        }



        private bool CheckCurrentHash(int userId, string str)
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
    }
}