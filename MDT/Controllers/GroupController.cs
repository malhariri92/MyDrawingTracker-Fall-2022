using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        const int INV_INDEX = 6;

        public ActionResult Index()
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];

            if (user != null && group != null && WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

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