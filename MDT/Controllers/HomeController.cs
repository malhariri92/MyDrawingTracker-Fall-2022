﻿using MDT.Filters;
using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace MDT.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private DbEntities db = new DbEntities();
        
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult NewUser()
        {

            return PartialView();
        }

        public ActionResult SignIn()
        {
            UserDTO user = (UserDTO)Session["User"];
            if (user == null)
            {
                 return PartialView();
                
            }
            return PartialView("GroupInfo");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(LoginDTO cred)
        {
            if (ModelState.IsValid)
            {
                cred.EmailAddress = cred.EmailAddress.ToLower();
                PasswordManager.AttemptLogin(cred);

                if (cred.LoginResult == PasswordManager.Result.SuccessfulLogin)
                {
                    SessionSetup(WebManager.GetUserDTO(cred.User.UserId));
                    string url = (string)Session["RedirectUrl"];
                    Session["RedirectUrl"] = null;
                    return Redirect(url ?? "/Home/Index");

                    return PartialView("UserAccess");
                   
                }

                if (cred.UserLocked)
                    ModelState.AddModelError("Password", "Too many failed sign in attempts. Account has been locked.");
                else
                    ModelState.AddModelError("Password", "The entered credentials are not valid.");
            }


            return PartialView(cred);
        }




        public ActionResult ForgotPass()
        {
            UserDTO user = (UserDTO)Session["User"];
            Console.WriteLine("TEst");
            if (user == null)
            {
                return PartialView("ForgotPass", new UserPasswordResetSetupVM());
            }
            return PartialView("Nope");
        }

        [HttpPost]
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

                    { "[[name]]", user.UserName },
                    { "[[key]]", key },
                };

                EmailMessage email = new EmailMessage();
                email.AddTo(user.EmailAddress);
                email.SetSubject("Password Reset Request");
                email.SetTemplateBody("ForgotPass.html", variables);
               
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
                    vm.Error=true;
                }
         

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
                User userViaKey = db.Users.Where(uk => uk.ResetKey.Equals(k)).FirstOrDefault();
                if (userViaKey == null)
                {
                    vm.Success = false;
                    vm.Error = true;
                    vm.Message = "Invalid authentication key. Please try again.";
                    return View(vm);
                }

                if (userViaKey.ResetKey == null)
                {
                    return RedirectToAction("ForgotPass");
                }

                if (userViaKey.ResetKeyExpires < DateTime.Now)
                {
                    vm.Success = false;
                    vm.Error = true;
                    vm.Message = "Key has expired. Please request a new key.";
                    return View(vm);
                }

                Session["UserKey"] = key;

                UserDTO uvkDTO = new UserDTO(userViaKey);
                Session["User"] = uvkDTO;
                vm.IsChangeRequest = true;

                return View("ResetPass", vm);
            }
            return RedirectToAction("ChangePass", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPass(UserPasswordResetVM vm)
        {
            UserDTO user = (UserDTO)Session["User"];

            if (User == null)
            {
                User key = (User)Session["UserKey"];


                if (key == null)
                {
                    return View("Nope");
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

                    key.ResetKey = null;
                    key.ResetKeyExpires = null;
                    db.Entry(key).State = EntityState.Modified;
                    db.SaveChanges();

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
            HttpContext.User = null;
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            Response.Cookies.Add(new HttpCookie(System.Web.Helpers.AntiForgeryConfig.CookieName) { Expires = DateTime.Now.AddMilliseconds(1) });
            Response.Cookies.Add(new HttpCookie("_mdt_", ""));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(GroupUserVM vm)
        {
            if (db.Users.Any(u => u.EmailAddress.Equals(vm.EmailAddress)))
            {
                ModelState.AddModelError("EmailAddress", "Email addrress is already in use.");
            }

            Group groupMatch = db.Groups.Where(g => g.AccessCode.Equals(vm.AccessCode)).FirstOrDefault();
            if (groupMatch == null)
            {
                ModelState.AddModelError("AccessCode", "Invalid Group Access Code.");
            }

            if (!ModelState.IsValid)
            {
                return PartialView(vm);
            }
            User user = new User()
            {
                UserName = vm.UserName,
                EmailAddress = vm.EmailAddress,
                CurrentGroupId = groupMatch.GroupId,
                IsActive = true,
                IsVerified = false,
            };

            db.Users.Add(user);

            Dictionary<string, string> variables = new Dictionary<string, string>()
            {
                { "[[Name]]", user.UserName },
                { "[[VerifyKey]]", key },
            };

            WebManager.SendTemplateEmail($"{user.EmailAddress}\t{user.UserName}", 2, variables);

            WebManager.SendTemplateEmail($"{user.EmailAddress}\t{user.UserName}", 2, variables);
            PasswordManager.SetNewHash(user.UserId, vm.Password);

            //Create the GroupUser entry
            var newGroupUser = new GroupUser()
            {
                GroupId = groupMatch.GroupId,
                UserId = user.UserId,
                IsAdmin = false,
            };
            db.GroupUsers.Add(newGroupUser);

            SessionSetup(WebManager.GetUserDTO(user.UserId));
            return PartialView("UnverifiedEmail");
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(AdminUserVM vm)
        {

            User user = db.Users.Where(u => u.EmailAddress.Equals(vm.EmailAddress)).FirstOrDefault();
            if (user != null)
            {
                ModelState.AddModelError("EmailAddress", "Email addrress is already in use.");
            }

            if (!ModelState.IsValid)
            {
                return PartialView(vm);
            }

            // Create a new group
            Group group = new Group()
            {
                GroupName = vm.GroupName,
                IsActive = true,
                IsPrimary = true,
                JoinConfirmationRequired = true,
            };

            db.Groups.Add(group);

            Dictionary<string, string> variables = new Dictionary<string, string>()
            {
                { "[[GroupName]]", group.GroupName }
            };
            List<string> recipients = db.GroupUsers.Where(gu => gu.GroupId == 0 && gu.IsAdmin).Select(gu => gu.User).ToList().Select(u => $"{u.EmailAddress}\t{u.UserName}").ToList();
            WebManager.SendTemplateEmail(recipients, 3, variables);

            WebManager.SendTemplateEmail(recipients, 3, variables);

            // Create a new user
            user = new User()
            {
                UserName = vm.UserName,
                EmailAddress = vm.EmailAddress,
                CurrentGroupId = group.GroupId,
                IsActive = true,
                IsVerified = false,
            };
            db.Users.Add(user);
            string key = WebManager.GetUniqueKey(10);
            db.VerificationKeys.Add(new VerificationKey()
            {
                UserId = user.UserId,
                EmailAddress = user.EmailAddress,
                VKey = key,
                SentOn = DateTime.Now
            });
            db.SaveChanges();

            variables = new Dictionary<string, string>()
            {
                { "[[Name]]", user.UserName },
                { "[[VerifyKey]]", key },
            };

            WebManager.SendTemplateEmail($"{user.EmailAddress}\t{user.UserName}", 2, variables);

            WebManager.SendTemplateEmail( $"{user.EmailAddress}\t{user.UserName}", 2, variables);

            //Hash the password and add to the newly created user.
            //user = db.Users.Where(u => u.EmailAddress.Equals(vm.EmailAddress)).FirstOrDefault();
            PasswordManager.SetNewHash(user.UserId, vm.Password);

            //Create the admin group user.
            var groupUser = new GroupUser()
            {
                GroupId = group.GroupId,
                UserId = user.UserId,
                IsAdmin = true,
            };
            db.GroupUsers.Add(groupUser);
            db.SaveChanges();

            ViewBag.SuccessMessage = "Your account has been created successfully!";
            ModelState.Clear();

            SessionSetup(WebManager.GetUserDTO(user.UserId));

        public ActionResult SendVerification()
        {
            UserDTO user = (UserDTO)Session["User"];
            string key = WebManager.GetUniqueKey(10);
            db.VerificationKeys.Add(new VerificationKey()
            {
                UserId = user.UserId,
                EmailAddress = user.EmailAddress,
                VKey = key,
                SentOn = DateTime.Now
            });
            db.SaveChanges();


            Dictionary<string, string> variables = new Dictionary<string, string>()
            {
                { "[[Name]]", user.UserName },
                { "[[VerifyKey]]", key },
            };

            WebManager.SendTemplateEmail($"{user.EmailAddress}\t{user.UserName}", 2, variables);
            return PartialView("VerificationEmailSent");
        }

        [LoginFilter]
        public ActionResult Verify(string key)
        {
            UserDTO user = (UserDTO)Session["User"];
            User u = db.Users.Find(user.UserId);
            VerificationKey vk = u.VerificationKeys.Where(k => k.VKey.Equals(key, StringComparison.CurrentCultureIgnoreCase) &&
                                                               k.EmailAddress.Equals(user.EmailAddress, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (vk != null)
            {
                vk.VerifiedOn = DateTime.Now;
                u.IsVerified = true;
                db.Entry(vk).State = EntityState.Modified;
                db.Entry(u).State = EntityState.Modified;
                db.SaveChanges();
                TempData["VerificationSuccess"] = $"Email address {user.EmailAddress} has been verified.";
                Session["User"] = new UserDTO(u);
            }
            else
            {
                if (u.VerificationKeys.Any(k => k.VKey.Equals(key, StringComparison.CurrentCultureIgnoreCase) || k.EmailAddress.Equals(user.EmailAddress, StringComparison.CurrentCultureIgnoreCase)))
                {
                    TempData["VerificationFailure"] = $"The email address you are attempting to verify does not match your current email address.";
                }
            }

            return RedirectToAction("Index", "Home", null);
        }

            return PartialView("VerificationEmailSent");
        }
        public void SessionSetup(UserDTO user)
        {
            string role;

            if (WebManager.IsGroupAdmin(0, user.UserId))
            {
                role = "Site Admin";
            }
            else
            {
                if (WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId))
                {
                    role = "Admin";
                }
                else
                {
                    role = "User";
                }
            }
            
            Session["User"] = WebManager.GetUserDTO(user.UserId);
            Session["Group"] = WebManager.GetGroupDTO(user.CurrentGroupId);
            Session["Ident"] = new GenericPrincipal(new GenericIdentity(user.EmailAddress), new string[] { role });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}