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

namespace MDT.Controllers
{
    public class UserController : BaseController
    {
        
        public ActionResult Index()
        {
            return View(user);
        }

        public bool ChangeGroup(int groupId)
        {
            if (WebManager.IsGroupMember(groupId, user.UserId))
            {
                Session["Group"] = WebManager.GetGroupVM(groupId);
            }

            return true;
        }

        public ActionResult ChangePass()
        {
            UserVM user = (UserVM)Session["User"];
            if (user != null)
            {
                return View(new UserPasswordChangeVM());
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [LoginFilter]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(UserPasswordChangeVM vm)
        {
            UserVM user = (UserVM)Session["User"];
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