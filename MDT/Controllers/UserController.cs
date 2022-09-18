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
            UserVM vm = new UserVM(db.Users.Where(u => user.UserId == u.UserId).Include(u => u.GroupUsers).FirstOrDefault());
            return View(vm);
        }

        public bool ChangeGroup(int groupId)
        {
            if (WebManager.IsGroupMember(groupId, user.UserId))
            {
                User u = db.Users.Find(user.UserId);
                u.CurrentGroupId = groupId;
                db.Entry(u).State = EntityState.Modified;
                db.SaveChanges();
                Session["User"] = new UserDTO(u);
                Session["Group"] = WebManager.GetGroupDTO(groupId);

                return true;
            }

            return false;
        }

        public ActionResult ChangePass()
        {
            if (user != null)
            {
                return View(new UserPasswordChangeVM());
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(UserPasswordChangeVM vm)
        {

            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            if (vm.CurrentPassword.Equals(vm.NewPassword))
            {
                vm.Success = false;
                ModelState.AddModelError("NewPassword", "New password must be different from current password.");
                return View(vm);
            }

            if (!WebManager.CheckCurrentHash(user.UserId, vm.CurrentPassword))
            {
                vm.Success = false;
                ModelState.AddModelError("CurrentPassword", "Current password incorrect.");
                return View(vm);
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
            return View(vm);
        }

    }
}