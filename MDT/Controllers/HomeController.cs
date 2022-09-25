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
                    { "[[authUrl]]", $"https://mydrawingtracker.com/Home/ResetPass?k={key}" },
                    { "[[key]]", key},
                    { "[[UserEmail]]", vm.UserEmail},
                    { "[[greeting]]", $"Hello {user.UserName}," },
                    { "[[body1]]", "We have received a password reset request for the My Drawing Tracker account associated with this e-mail address. " +
                        "If you have not made a password reset request for your My Drawing Tracker account, you may safely ignore this e-mail. If you " +
                        "have made a password request, please "},
                    { "[[body2]]", " within one hour of receiving this message in order to change your password." },
                    { "[[TemplateName]]", "Password Reset Request" },
                };

                EmailMessage email = new EmailMessage();
                email.AddTo(user.EmailAddress);
                email.SetSubject("Password Reset Request");
                email.SetTemplateBody("ForgotPass.html", variables);
               
                List<string> recipients = new List<string>();
                recipients.Add(user.EmailAddress);
                WebManager.SendTemplateEmail(
                    recipients, 
                    1,
                    variables
                );
         

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
                User key = db.Users.Where(uk => uk.ResetKey.Equals(k)).FirstOrDefault();
                if (key == null)
                {
                    vm.Success = false;
                    vm.Error = true;
                    vm.Message = "Invalid authentication key. Please try again.";
                    return View(vm);
                }

                    if (key.ResetKey == null)
                    {
                        return RedirectToAction("ForgotPass");
                    }

                    if (key.ResetKeyExpires < DateTime.Now)
                    {
                        vm.Success = false;
                        vm.Error = true;
                        vm.Message = "Key has expired. Please request a new key.";
                        return View(vm);
                    }

                Session["UserKey"] = key;

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
            db.SaveChanges();

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
            Session["RedirectUrl"] = null;
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