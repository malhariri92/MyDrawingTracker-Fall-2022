using MDT.Filters;
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

        public ActionResult Index(string code = null)
        {
            if (code != null)
            {
                Session["AccessCode"] = code;
            }

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData["Message"] = null;
            }

            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
                TempData["Error"] = null;
            }
            return View();
        }

        public ActionResult NewUser()
        {
            string code = (string)Session["AccessCode"];
            NewUserVM vm = new NewUserVM()
            {
                CreateAdmin = true,
                AccessCode = code
            };

            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewUser(NewUserVM vm)
        {
            if (db.Users.Any(u => u.EmailAddress.Equals(vm.EmailAddress)))
            {
                ModelState.AddModelError("EmailAddress", "Email addrress is already in use.");
            }
            Group group = null;
            if (vm.CreateAdmin)
            {
                ModelState.Remove("AccessCode");
            }
            else
            {
                ModelState.Remove("GroupName");
                ModelState.Remove("Reason");
                group = db.Groups.Where(g => g.AccessCode.Equals(vm.AccessCode)).FirstOrDefault();
                if (group == null)
                {
                    ModelState.AddModelError("AccessCode", "Invalid Group Access Code.");
                }
            }
            
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("NewUser", vm);
            }

            Dictionary<string, string> variables;

            if (vm.CreateAdmin)
            {
                group = new Group()
                {
                    GroupName = vm.GroupName,
                    IsActive = true,
                    AccessCode = WebManager.GetUniqueKey(10),
                    JoinConfirmationRequired = true,
                    AccountBalanceLedger = new Ledger()
                    {
                        LedgerName = "Account Balance",
                        Balance = 0.0m
                    }
                };

                db.Groups.Add(group);
                db.SaveChanges();
                variables = new Dictionary<string, string>()
                {
                    { "[[GroupName]]", group.GroupName },
                    { "[[Reason]]", vm.Reason }
                };

                List<string> recipients = db.GroupUsers.Where(gu => gu.GroupId == 0 && gu.IsAdmin).Select(gu => gu.User).ToList().Select(u => $"{u.EmailAddress}\t{u.UserName}").ToList();
                WebManager.SendTemplateEmail(recipients, 3, variables);

                Description desc = new Description()
                {
                    ObjectTypeId = 5,
                    ObjectId = group.GroupId,
                    SortOrder = 1,
                    TextBody = vm.Reason,
                    IsHTML = false
                };

                db.Entry(desc).State = EntityState.Added;
            }

            User user = new User()
            {
                UserName = vm.UserName,
                EmailAddress = vm.EmailAddress,
                CurrentGroupId = group.GroupId,
                IsActive = true,
                IsVerified = false,
            };

            db.Users.Add(user);
            db.SaveChanges();
            PasswordManager.SetNewHash(user.UserId, vm.Password);

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

            //Create the GroupUser entry
            GroupUser newGroupUser = new GroupUser()
            {
                GroupId = group.GroupId,
                UserId = user.UserId,
                IsAdmin = vm.CreateAdmin,
                IsApproved = vm.CreateAdmin || !group.JoinConfirmationRequired,
            };
            db.GroupUsers.Add(newGroupUser);
            db.SaveChanges();

            if (!vm.CreateAdmin)
            {
                //Generate notification email
                User groupAdmin = db.GroupUsers.Where(u => u.GroupId == group.GroupId && u.IsAdmin).Select(gu => gu.User).FirstOrDefault();
                variables = new Dictionary<string, string>()
                {
                    { "[[Name]]", groupAdmin.UserName },
                    { "[[GroupName]]", group.GroupName },
                    { "[[UserName]]", user.UserName },
                    { "[[ConfirmUrl]]", "Group/Index" }
                };
                
                WebManager.SendTemplateEmail($"{groupAdmin.EmailAddress}\t{groupAdmin.UserName}", group.JoinConfirmationRequired ? 8 : 7, variables);
            }

            SessionSetup(WebManager.GetUserDTO(user.UserId));
            ViewBag.IsNewUser = true;
            return PartialView("SuccessfulSignIn", user);
        }

        public ActionResult SignIn()
        {
            UserDTO user = (UserDTO)Session["User"];
            if (user == null)
            {
                return PartialView();
            }

            return PartialView("SuccessfulSignIn", db.Users.Find(user.UserId));
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

                    ViewBag.IsNewUser = false;
                    return PartialView("SuccessfulSignIn", cred.User);

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
                    { "[[Key]]", key},
                    { "[[Name]]", user.UserName },
                };

                WebManager.SendTemplateEmail($"{user.EmailAddress}\t{user.UserName}", 1, variables);


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

            if (user == null)
            {
                User key = (User)Session["UserKey"];


                if (key == null)
                {
                    return View("Nope");
                }


                if (key.ResetKey == null)
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
                    Response.StatusCode = 400;
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
                TempData["Message"] = $"Email address {user.EmailAddress} has been verified.";
                SessionSetup(new UserDTO(db.GroupUsers.Where(gu => gu.GroupId == u.CurrentGroupId && gu.UserId == u.UserId).Include(gu => gu.User).FirstOrDefault()));
            }
            else
            {
                if (u.VerificationKeys.Any(k => k.VKey.Equals(key, StringComparison.CurrentCultureIgnoreCase) || k.EmailAddress.Equals(user.EmailAddress, StringComparison.CurrentCultureIgnoreCase)))
                {
                    TempData["Error"] = $"The email address you are attempting to verify does not match your current email address.";
                }
            }

            return RedirectToAction("Index", "Home", null);
        }

        public ActionResult Nav()
        {
            UserDTO user = (UserDTO)Session["User"];
            if (user != null)
            {
                ViewBag.Groups = db.GroupUsers.Where(g => g.UserId == user.UserId).Select(g => g.Group).ToList().Select(g => new DdlItem(g.GroupId, g.GroupName)).ToList();
            }
            else
            {
                ViewBag.Groups = new List<DdlItem>();
            }
            return PartialView();
        }

        public ActionResult Banner()
        {
            UserDTO user = (UserDTO)Session["User"];
            if (user != null)
            {
                ViewBag.Groups = db.GroupUsers.Where(g => g.UserId == user.UserId).Select(g => g.Group).ToList().Select(g => new DdlItem(g.GroupId, g.GroupName)).ToList();
            }
            else
            {
                ViewBag.Groups = new List<DdlItem>();
            }
            return PartialView();
        }

        public ActionResult GroupNav()
        {
            UserDTO user = (UserDTO)Session["User"];
            int GroupId = user?.CurrentGroupId ?? -1;
            List<DrawType> dts = db.DrawTypes.Where(dt => dt.GroupId == GroupId)
                                             .Include(dt => dt.Draws)
                                             .Include(dt => dt.Draws.Select(d => d.DrawType))
                                             .ToList();

            GroupNavVM vm = new GroupNavVM(dts);
            return PartialView(vm);
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
            Session["VerifiedUser"] = WebManager.GetUserDTO(user.UserId).IsVerified;
            Session["ApprovedGroup"] = (WebManager.GetGroupDTO(user.CurrentGroupId).IsApproved ?? false);
            Session["Ident"] = new GenericPrincipal(new GenericIdentity(user.EmailAddress), new string[] { role });
        }

        [LoginFilter]
        public ActionResult ChangeGroup(int groupId)
        {
            UserDTO user = (UserDTO)Session["User"];
            if (user.CurrentGroupId != groupId && db.GroupUsers.Find(groupId, user.UserId) != null)
            {
                GroupUser u = db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId && gu.UserId == user.UserId).Include(gu => gu.User).FirstOrDefault();
                u.User.CurrentGroupId = groupId;
                db.Entry(u.User).State = EntityState.Modified;
                db.SaveChanges();
                Session["User"] = new UserDTO(u);
                Session["Group"] = WebManager.GetGroupDTO(groupId);
            }

            return RedirectToAction("Index", "Group", null);
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