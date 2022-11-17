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
using System.Text.RegularExpressions;

namespace MDT.Controllers
{
    public class UserController : BaseController
    {

        public ActionResult Index()
        {
            UserVM vm = GetUserVM();
            ViewBag.Draws = GetDraws();
            return View(vm);
        }

        public ActionResult IndexPartial()
        {
            UserVM vm = GetUserVM();
            ViewBag.Draws = GetDraws();
            return PartialView("Index", vm);
        }

        private UserVM GetUserVM()
        {
            return new UserVM(db.Users.Where(u => u.UserId == user.UserId).Include(u => u.GroupUsers).FirstOrDefault(), group.GroupId);
        }

        private List<Draw> GetDraws()
        {
            return db.Draws
                .Where(d => d.DrawType.GroupId == group.GroupId && d.EndDateTime > DateTime.Now)
                .ToList();
        }


        public ActionResult ChangePass()
        {
            if (user != null)
            {
                return PartialView(new UserPasswordChangeVM());
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(UserPasswordChangeVM vm)
        {

            if (!ModelState.IsValid)
            {
                return PartialView(vm);
            }
            if (vm.CurrentPassword.Equals(vm.NewPassword))
            {
                vm.Success = false;
                ModelState.AddModelError("NewPassword", "New password must be different from current password.");
                return PartialView(vm);
            }

            if (!CheckCurrentHash(user.UserId, vm.CurrentPassword))
            {
                vm.Success = false;
                ModelState.AddModelError("CurrentPassword", "Current password incorrect.");
                return PartialView(vm);
            }

            if (PasswordManager.SetNewHash(user.UserId, vm.NewPassword))
            {
                vm.Success = true;
            }
            else
            {
                vm.Success = false;
                vm.Error = true;
                vm.Message = "Something went wrong updating your password. Please try again.";
            }
            return PartialView(vm);
        }

        public ActionResult Edit()
        {
            if (user != null)
            {
                user = (UserDTO)Session["User"];
                List<int> ids = db.GroupUsers.Where(g => g.UserId == user.UserId).Select(g => g.GroupId).ToList();
                List<DdlItem> groups = db.Groups.ToList().Where(g => ids.Contains(g.GroupId))
                    .Select(g => new DdlItem(g.GroupId, g.GroupName)).ToList();
                ViewBag.Groups = groups;

                return PartialView(new UserDetailsChangeVM(user));
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserDetailsChangeVM vm)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView(vm);
            }

            GroupDTO groupDTO = WebManager.GetGroupDTO(vm.CurrentGroupId);
            if (groupDTO == null || String.IsNullOrEmpty(groupDTO.GroupName))
            {
                vm.Success = false;
                vm.Error = true;
                vm.Message = "Invalid GroupId";
                return PartialView(vm);
            }

            try
            {
                    if (user == null)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "User Retrieval Error (1)";
                        return PartialView(vm);
                    }

                    user.UserName = vm.UserName;
                    // user.PhoneNumber = vm.PhoneNumber;
                    user.CurrentGroupId = vm.CurrentGroupId;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    Session["User"] = new UserDTO(vm);
                    Session["Group"] = groupDTO;
               
            }
            catch
            {
                vm.Success = false;
                vm.Error = true;
                vm.Message = "User Retrieval Error (2)";
                return View(vm);
            }

            vm.Success = true;
            vm.Error = false;
            return View(vm);

        }

        public ActionResult Member(int id)
        {
            ViewBag.IsOwner = db.GroupUsers.Where(u => u.UserId == id && u.GroupId == user.CurrentGroupId && u.IsOwner == true).Any();
            ViewBag.IsAdmin = db.GroupUsers.Where(u => u.UserId == id && u.GroupId == user.CurrentGroupId && u.IsAdmin == true).Any();
            return View(new UserVM(db.Users.Where(u => u.UserId == id)
                                      .Include(u => u.GroupUsers)
                                      .Include(u => u.Balances)
                                      .Include(u => u.Balances.Select(b => b.Ledger))
                                      .FirstOrDefault(), group.GroupId));
        }

        public ActionResult JoinGroupWithCode()
        {
            return PartialView();
        }

        [HttpPost]

        public ActionResult JoinGroupWithCode(string AccessCode = "")
        {
            if (AccessCode.Equals(""))
            {
                ViewBag.Error = "You must enter a group access code!";
                return PartialView();
            }

            MDT.Models.Group grp = db.Groups.Where(g => g.AccessCode == AccessCode).FirstOrDefault();

            if (grp == null)
            {
                ViewBag.Error = $"No group exists with the access code {AccessCode}!";
            }
            else if (db.GroupUsers.Where(gu => gu.UserId == user.UserId && gu.GroupId == grp.GroupId).Any())
            {
                ViewBag.Error = $"You are already a member of {grp.GroupName}!";
            }
            else
            {
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

                ViewBag.SuccessMessage =
                    grp.JoinConfirmationRequired
                    ? $"A request to join {grp.GroupName} has been sent!"
                    : $"You have successfully been added to {grp.GroupName}!";

            }
            return PartialView();
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