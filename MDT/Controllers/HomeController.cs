using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MDT.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {

            return View();
        }

        [AllowAnonymous]
        public ActionResult SignIn()
        {
            UserDTO user = (UserDTO)Session["User"];
            if (user == null)
            {
                using (var db = new DbEntities())
                {
                    if (Request.IsAjaxRequest())
                    {
                        return PartialView();
                    }
                    else
                    {
                        return View();
                    }
                }
            }
            string url = (string)Session["RedirectUrl"] ?? "/Home/Index";
            Session["RedirectUrl"] = null;

            return Redirect(url);

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(LoginDTO cred)
        {
            if (ModelState.IsValid)
            {
                cred.EmailAddress = cred.EmailAddress.ToLower();
                PasswordManager.AttemptLogin(cred);

                if (cred.LoginResult == PasswordManager.Result.SuccessfulLogin)
                {
                    User user = cred.User;
                    string url = (string)Session["RedirectUrl"] ?? "/User/Index";
                    Session["RedirectUrl"] = null;
                    using (var db = new DbEntities())
                    {
                        Session["User"] = WebManager.GetUserDTO(user.UserId);
                        Session["Group"] = WebManager.GetGroupDTO(user.CurrentGroupId);

                    }

                    return Redirect(url);
                }

                if (cred.UserLocked)
                    ModelState.AddModelError("Password", "Too many failed sign in attempts. Account has been locked.");
                else
                    ModelState.AddModelError("Password", "The entered credentials are not valid.");
            }


            return View(cred);
        }



        [AllowAnonymous]
        public ActionResult ForgotPass()
        {
            UserDTO user = (UserDTO)Session["User"];
            if (user == null)
            {
                return View("ForgotPass", new UserPasswordResetSetupVM());
            }
            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPass(UserPasswordResetSetupVM vm)
        {

            UserDTO user = WebManager.GetUserDTOByEmail(vm.UserEmail);
            vm.UserId = user?.UserId ?? 0;

            vm.UserName = user?.UserName ?? "";

            string key = WebManager.GenerateUserKey(vm);
            if (key != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[authUrl]]", $"https://mydrawingtracker.com/Home/ResetPass?k={key}" },
                    { "[[key]]", key},
                    { "[[UserEmail]]", vm.UserEmail},
                };

                EmailMessage email = new EmailMessage();
                email.AddTo(user.EmailAddress);
                email.SetSubject("Password Reset Request");
                email.SetTextOnlyBody($"https://mydrawingtracker.com/Home/ResetPass?k={key}");
                email.SendMessage();

            }
            vm.Success = true;

            return PartialView("ForgotPass", vm);

        }

        [AllowAnonymous]
        public ActionResult ResetPass(string k)
        {
            UserDTO user = (UserDTO)Session["User"];
            if (user == null)
            {
                UserPasswordResetVM vm = new UserPasswordResetVM();
                using (var db = new DbEntities())
                {
                    User key = db.Users.Where(uk => uk.ResetKey.Equals(k)).FirstOrDefault();
                    if (key == null)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Invalid authentication key. Please try again.";
                        return View(vm);
                    }

                    if (key.ResetKeyExpires < DateTime.Now)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Key has expired. Please request a new key.";
                        return View(vm);
                    }

                    Session["UserKey"] = key;

                }
                return View("ResetPass", vm);
            }
            return RedirectToAction("ChangePass");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPass(UserPasswordResetVM vm)
        {
            UserDTO user = (UserDTO)Session["User"];
            if (User == null)
            {
                User key = (User)Session["UserKey"];


                if (key == null)
                {
                    return View("Unauthorized");
                }


                if (key.ResetKey != null)
                {
                    vm.Success = false;
                    vm.Error = true;
                    vm.Message = "Key has been used. Please request a new key.";
                    return View(vm);
                }

                if (key.ResetKeyExpires < DateTime.Now)
                {
                    vm.Success = false;
                    vm.Error = true;
                    vm.Message = "Key has expired. Please request a new key.";
                    return View(vm);
                }


                if (!ModelState.IsValid)
                {
                    Session["UserKey"] = key;
                    return View(vm);
                }



                if (PasswordManager.SetNewHash(key.UserId, vm.NewPassword))
                {
                    vm.Success = true;
                    using (var db = new DbEntities())
                    {
                        key.ResetKey = null;
                        key.ResetKeyExpires = null;
                        db.Entry(key).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else
                {
                    vm.Success = false;
                    vm.Error = true;
                    vm.Message = "Something went wrong updating your password. Please try again.";
                    Session["UserKey"] = key;
                }
                return View(vm);
            }

            return RedirectToAction("ChangePass");
        }

        public ActionResult SignOut()
        {
            Session["SignedOutUser"] = Session["User"];
            //SessionSetup(null);
            HttpContext.User = null;
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            Response.Cookies.Add(new HttpCookie(System.Web.Helpers.AntiForgeryConfig.CookieName) { Expires = DateTime.Now.AddMilliseconds(1) });
            Response.Cookies.Add(new HttpCookie("_z_", ""));

            return RedirectToAction("Index", "Home");
        }
    }
}