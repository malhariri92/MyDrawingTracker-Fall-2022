using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;

namespace MDT.Controllers
{
    public class GroupController : BaseController
    {

        // GET: Group
        [HttpGet]
        public ActionResult GroupIndex()
        {
            UserDTO user = (UserDTO)Session["User"];
            GroupDTO group = (GroupDTO)Session["Group"];
            if (!WebManager.IsGroupAdmin(group.GroupId, user.UserId))
            {
                return RedirectToAction("Index", "User");
            }

            if (group.JoinConfirmationRequired)
            {
                List<User> users = db.Users.Where(u => u.CurrentGroupId == group.GroupId && !u.IsActive).ToList();
                ViewBag.users = users;
            }

            return View();
        }

        [HttpPost]
        public ActionResult Reject(UserVM vm)
        {
            User user = db.Users.Find(vm.UserId);
            user.IsActive = false;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return View();
        }

        [HttpPost]
        public ActionResult Approve(UserVM vm)
        {
            User user = db.Users.Find(vm.UserId);
            GroupDTO group = WebManager.GetGroupDTO(user.CurrentGroupId);

            user.IsActive = true;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[userName]]", user.UserName },
                    { "[[groupName]]", group.GroupName },
                    { "[[TemplateName]]", "New Group User Apprroved" },
                };

            EmailMessage email = new EmailMessage();
            email.AddTo(user.EmailAddress);
            email.SetSubject("You Request Has Been Approved");
            email.SetTemplateBody("UserApproved.html", variables);

            List<string> recipients = new List<string>();
            recipients.Add(user.EmailAddress);
            WebManager.SendTemplateEmail(
                recipients,
                9,
                variables
            );

            return View();
        }

    }
}