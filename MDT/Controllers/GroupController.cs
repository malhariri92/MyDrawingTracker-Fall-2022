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
using Newtonsoft.Json;
using System.Data.Entity;
using System.Drawing.Printing;
using System.Linq;
using System.Media;
using System.Web;
using System.Web.Mvc;

namespace MDT.Controllers
{
    public class GroupController : BaseController
    {


        public ActionResult Index()
        {
            using (var db = new DbEntities())
            {
                List<User> ml = db.GroupUsers.Where(gu => gu.GroupId == user.CurrentGroupId).Select(gu => gu.User).ToList();
                List<UserDTO> mld = new List<UserDTO>();
                foreach (User u in ml)
                {
                    if (user.UserId == u.UserId)
                    {
                        continue;
                    }
                    GroupUser gu = db.GroupUsers.Where(g => g.UserId == u.UserId && g.GroupId == u.CurrentGroupId).FirstOrDefault();

                    mld.Add(new UserDTO(u));
                }
                return View(mld);
            }
        }

        public ActionResult RemoveFromGroup(int uId, int guId)
        {
            using (var db = new DbEntities())
            {
                try
                {
                    GroupUser gremoved = db.GroupUsers.Where(r => r.UserId == uId && r.GroupId == guId).First();
                    GroupUser gremoved2 = db.GroupUsers.Remove(gremoved);

                    db.Entry(gremoved2).State = EntityState.Deleted;
                    db.SaveChanges();
                }
                catch{}
            }
            return RedirectToAction("Index", "Group");
        const int INV_INDEX = 6;


        public ActionResult InviteList()
        {
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                return View(getInvites(group.GroupId));
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendInvite(GroupInvite grpInvite)
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                GroupInvite dBGroupInvite =
                db.GroupInvites.Where(gI => grpInvite.EmailAddress == gI.EmailAddress && group.GroupId == gI.GroupId).FirstOrDefault();

                if (dBGroupInvite == null)
                {
                    grpInvite.LastInviteDate = DateTime.Now;
                    grpInvite.GroupId = group.GroupId;
                    grpInvite.InviteCount = 1;
                    db.Entry(grpInvite).State = EntityState.Added;
                    db.SaveChanges();
                }

                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[sender]]", user.UserName },
                    { "[[groupName]]", group.GroupName },
                };

                List<string> recipients = new List<string>();
                recipients.Add(grpInvite.EmailAddress);

                WebManager.SendTemplateEmail(recipients, INV_INDEX, variables);

                return PartialView(grpInvite);
            }

            return RedirectToAction("Index", "Home");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TriggerPasswordReset(string email)
        {

            UserDTO user = WebManager.GetUserDTOByEmail(email);
            UserPasswordResetSetupVM vm = new UserPasswordResetSetupVM();

            vm.UserId = user.UserId;
            vm.UserEmail = user.EmailAddress;

            string key = WebManager.GenerateUserKey(vm);
            if (key != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[authUrl]]", $"https://mydrawingtracker.com/Home/ResetPass?k={key}" },
                    //{ "[[authUrl]]", $"https://localhost:44361/Home/ResetPass?k={key}" },// for testing purposes
                    { "[[key]]", key},
                    { "[[UserEmail]]", vm.UserEmail},
                    { "[[greeting]]", $"Hello {user.UserName}," },
                    { "[[body1]]", "We have received a password reset request for the My Drawing Tracker account associated with this e-mail address. " +
                        "If you have not made a password reset request for your My Drawing Tracker account, you may safely ignore this e-mail. If you " +
                        "have made a password request, please "},
                    { "[[body2]]", " within one hour of receiving this message in order to change your password." },
                    { "[[TemplateName]]", "Password Reset Request" },
                };

                EmailMessage em = new EmailMessage();
                em.AddTo(user.EmailAddress);
                em.SetSubject("Password Reset Request");
                em.SetTemplateBody("ForgotPass.html", variables);

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
                    vm.Error = true;
                }


            }

            return RedirectToAction("Index", "Group");
        public ActionResult SendReminder(GroupInvite grpInvite)
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                sendReminder(grpInvite.EmailAddress, group.GroupId);

                return PartialView(grpInvite);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult ConfirmDelete(GroupInvite grpInvite)
        {
            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                return PartialView("ConfirmDelete", grpInvite);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteInvite(GroupInvite grpInvite)
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                deleteInvitation(grpInvite.EmailAddress, group.GroupId);

                return PartialView(grpInvite);
            }
            return RedirectToAction("Index", "Home");
        }

        private List<GroupInvite> getInvites(int groupId)
        {
            return db.GroupInvites.Where(gI => gI.GroupId == group.GroupId).ToList();
        }

        private void sendReminder(string EmailAddress, int GroupId)
        {
            GroupInvite reminding = db.GroupInvites.Where(gI => gI.EmailAddress == EmailAddress && gI.GroupId == GroupId).FirstOrDefault();

            if (reminding != null)
            {
                reminding.LastInviteDate = DateTime.Now;
                db.Entry(reminding).State = EntityState.Modified;
                db.SaveChanges();

                Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[sender]]", user.UserName },
                    { "[[groupName]]", group.GroupName },
                };

                List<string> recipients = new List<string>();
                recipients.Add(EmailAddress);

                WebManager.SendTemplateEmail(recipients, INV_INDEX, variables);
            }
        }

        private void deleteInvitation(string EmailAddress, int GroupId)
        {
            GroupInvite delete = db.GroupInvites
                .Where(
                    gI => gI.GroupId == GroupId && gI.EmailAddress == EmailAddress)
                .FirstOrDefault();
            if (delete != null)
            {
                db.Entry(delete).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }
    }
}