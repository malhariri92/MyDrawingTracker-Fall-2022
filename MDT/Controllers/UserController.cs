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
            return new UserVM(db.Users.Where(u => u.UserId == user.UserId).Include(u => u.GroupUsers).FirstOrDefault());
        }

        private List<Draw> GetDraws()
        {
            return db.Draws
                .Where(d => d.DrawType.GroupDrawTypes.Any(g => g.GroupId == group.GroupId) && d.EndDateTime > DateTime.Now)
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

        public ActionResult ChangeUserDetails()
        {
            if (user != null)
            {
                user = (UserDTO) Session["User"];
                List<int> ids = db.GroupUsers.Where(g => g.UserId == user.UserId).Select(g => g.GroupId).ToList();
                List<DdlItem> groups = db.Groups.ToList().Where(g => ids.Contains(g.GroupId))
                    .Select(g => new DdlItem(g.GroupId, g.GroupName)).ToList();
                ViewBag.Groups = groups;

                return View(new UserDetailsChangeVM(user));
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeUserDetails(UserDetailsChangeVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            /*if (vm.UserName.Length < 6)
            {
                vm.Success = false;
                vm.Error = true;
                vm.Message = "Please make sure that your username is more than 1 characters.";
                return View(vm);
            }

            if (vm.UserName.Length > 25)
            {
                vm.Success = false;
                vm.Error = true;
                vm.Message = "Please make sure that your username is at most 25 characters.";
                return View(vm);
            }*/

            GroupDTO groupDTO = WebManager.GetGroupDTO(vm.CurrentGroupId);
            if (groupDTO == null || String.IsNullOrEmpty(groupDTO.GroupName))
            {
                vm.Success = false;
                vm.Error = true;
                vm.Message = "Invalid GroupId";
                return View(vm);
            }

            try
            {
                using (var db = new DbEntities())
                {
                    User user = db.Users.Find(vm.UserId);
                    if (user == null)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "User Retrieval Error (1)";
                        return View(vm);
                    }

                    user.UserName = vm.UserName;
                    // user.PhoneNumber = vm.PhoneNumber;
                    user.CurrentGroupId = vm.CurrentGroupId;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    Session["User"] = new UserDTO(vm);
                    Session["Group"] = groupDTO;
                }
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