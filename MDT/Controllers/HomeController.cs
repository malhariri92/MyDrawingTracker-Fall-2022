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

                UserDTO userDTO = new UserDTO(userViaKey);
                Session["User"] = userDTO;
                Session["UserKey"] = userViaKey;
                Session["Group"] = WebManager.GetGroupDTO(userViaKey.CurrentGroupId);

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

            if (user != null)
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
                    key = db.Users.Find(key.UserId);
                    vm.Success = true;
                    vm.IsChangeRequest = true;
                    using (var db = new DbEntities())
                    {
                        key = db.Users.Find(key.UserId); //grab user as it is changed by SetNewHash function
                        vm.Success = true;
                        vm.IsChangeRequest = true;

                        key.ResetKey = null;
                        key.ResetKeyExpires = null;
                        db.Entry(key).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                }
                else
                {
                    vm.Success = false;
                    vm.Error = true;
                    vm.Message = "Something went wrong updating your password. Please try again.";
                    Session["UserKey"] = key;

                    return View(vm);
                }
                if(!(PasswordManager.UpdateReset(key.UserId)))
                {
                    vm.Success = false;
                    vm.Error = true;
                    vm.Message = "Something went wrong updating your password reset key status. Please try again.";
                    Session["UserKey"] = key;
                }
                
                return View(vm);
            }

            return RedirectToAction("ChangePass", "User");
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
            db.SaveChanges();

            PasswordManager.SetNewHash(user.UserId, vm.Password);

            //Generate notification email
            GroupUser groupUser = db.GroupUsers.Where(u =>  u.GroupId == groupMatch.GroupId && u.IsAdmin).FirstOrDefault();
            User groupAdmin = db.Users.Find(groupUser.UserId);

            string subject = groupMatch.JoinConfirmationRequired ? "Confirm New User" : "A New User Joined Your Group";
            string templateName = groupMatch.JoinConfirmationRequired ? "Confirm New Group User" : "New Group User";
            int templateId = groupMatch.JoinConfirmationRequired ? 8 : 7;

            Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[userName]]", user.UserName },
                    { "[[adminName]]", groupAdmin.UserName },
                    { "[[groupName]]", groupMatch.GroupName },
                    { "[[TemplateName]]", templateName },
                    { "[[confirmUrl]]", "" },
                };

            EmailMessage email = new EmailMessage();
            email.AddTo(groupAdmin.EmailAddress);
            email.SetSubject(subject);
            email.SetTemplateBody(templateName, variables);

            List<string> recipients = new List<string>();
            recipients.Add(groupAdmin.EmailAddress);
            WebManager.SendTemplateEmail(
                recipients,
                templateId,
                variables
            );

            //Create the GroupUser entry
            var newGroupUser = new GroupUser()
            {
                GroupId = groupMatch.GroupId,
                UserId = user.UserId,
                IsAdmin = false,
            };
            db.GroupUsers.Add(newGroupUser);
            db.SaveChanges();

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
            db.SaveChanges();


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
            db.SaveChanges();

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
            return PartialView("UnverifiedEmail");
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