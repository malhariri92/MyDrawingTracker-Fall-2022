using MDT.Filters;
using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MDT.Controllers
{
    [AdminFilter(Role = "Site Admin")]
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            List<GroupVM> vm = db.Groups.Include(g => g.GroupUsers)
                                        .Include(g => g.GroupUsers.Select(gu => gu.User))
                                        .Include(g => g.GroupInvites)
                                        .ToList()
                                        .Select(g => new GroupVM(g))
                                        .ToList();
            return View(vm);
        }

        public ActionResult AllGroups()
        {
            if (TempData.ContainsKey("Message"))
            {
                ViewBag.Message = TempData["Message"];
                TempData.Remove("Message");
            }

            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }


            List<GroupVM> vm = db.Groups.Where(g => g.IsApproved ?? false)
                                       .Include(g => g.GroupUsers)
                                       .Include(g => g.GroupUsers.Select(gu => gu.User))
                                       .Include(g => g.GroupInvites)
                                       .ToList()
                                       .Select(g => new GroupVM(g))
                                       .ToList();
            return PartialView(vm);
        }

        public ActionResult Applications()
        {
            if (TempData.ContainsKey("Message"))
            {
                ViewBag.Message = TempData["Message"];
                TempData.Remove("Message");
            }

            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }

            List<GroupVM> vm = db.Groups.Where(g => g.IsApproved == null)
                                        .Include(g => g.GroupUsers)
                                        .Include(g => g.GroupUsers.Select(gu => gu.User))
                                        .Include(g => g.GroupInvites)
                                        .ToList()
                                        .Select(g => new GroupVM(g))
                                        .ToList();

            foreach (GroupVM gvm in vm)
            {
                gvm.SetDescriptions(db.Descriptions.Where(d => new List<int>() { 1, 5, 6 }.Contains(d.ObjectTypeId) && d.ObjectId == gvm.GroupId).ToList());
            }


            return PartialView(vm);
        }

        public ActionResult Approved(int id)
        {
            Group g = GetGroup(id);
            if (g == null)
            {
                TempData["Error"] = $"Group id {id} not found";
            }

            if (g.IsApproved != null)
            {
                TempData["Error"] = $"Group: {g.GroupName} has already been {(g.IsApproved.Value ? "approved" : "rejected")}";
            }

            g.IsApproved = true;
            g.AccountBalanceLedger = new Ledger()
            {
                GroupId = g.GroupId,
                LedgerName = "Account Balance",
                Balance = 0.0m
            };

            db.Entry(g).State = EntityState.Modified;
            db.SaveChanges();

            User u = g.GroupUsers.Where(gu => gu.IsAdmin).Select(gu => gu.User).FirstOrDefault();

            Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[Name]]", u.UserName },
                    { "[[GroupName]]", g.GroupName },
                    { "[[GroupId]]", $"{g.GroupId}"},
                };

            WebManager.SendTemplateEmail($"{u.EmailAddress}\t{u.UserName}", 4, variables);

            TempData["Message"] = $"Group: {g.GroupName} has been {(g.IsApproved.Value ? "approved" : "rejected")}";
            return RedirectToAction("Applications");
        }

        public ActionResult Reject(int id)
        {
            GroupVM vm = GetGroupVM(id);
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rejected(GroupVM vm)
        {
            if(!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView(vm);
            }
            Group g = GetGroup(vm.GroupId);
            if (g == null)
            {
                TempData["Error"] = $"Group id {vm.GroupId} not found";
                return RedirectToAction("Applications");
            }

            if (g.IsApproved != null)
            {
                TempData["Error"] = $"Group:  {g.GroupName} has already been {(g.IsApproved.Value ? "approved" : "rejected")}";
                return RedirectToAction("Applications");
            }

            g.IsApproved = false;
            Description desc = new Description()
            {
                ObjectTypeId = 6,
                ObjectId = g.GroupId,
                TextBody = vm.TextArea,
                IsHTML = false
            };

            db.Entry(g).State = EntityState.Modified;
            db.Entry(desc).State = EntityState.Added;
            db.SaveChanges();

            User u = g.GroupUsers.Where(gu => gu.IsAdmin).Select(gu => gu.User).FirstOrDefault();

            Dictionary<string, string> variables = new Dictionary<string, string>()
                {
                    { "[[Name]]", u.UserName },
                    { "[[GroupName]]", g.GroupName },
                    { "[[RejectReason]]", desc.TextBody},
                };

            WebManager.SendTemplateEmail($"{u.EmailAddress}\t{u.UserName}", 5, variables);

            TempData["Message"] = $"Group: {g.GroupName} has been {(g.IsApproved.Value ? "approved" : "rejected")}";
            return PartialView("Applications");
        }

        public ActionResult Impersonate(string email)
        {
            UserDTO imp = WebManager.GetUserDTOByEmail(email);
            if (imp != null)
            {
                SessionSetup(imp);
            }

            return RedirectToAction("Index", "Home", null);
        }

        private void SessionSetup(UserDTO imp)
        {
            string role;

            if (WebManager.IsGroupAdmin(0, imp.UserId))
            {
                role = "Site Admin";
            }
            else
            {
                if (WebManager.IsGroupAdmin(imp.CurrentGroupId, imp.UserId))
                {
                    role = "Admin";
                }
                else
                {
                    role = "User";
                }
            }

            Session["User"] = WebManager.GetUserDTO(imp.UserId);
            Session["Group"] = WebManager.GetGroupDTO(imp.CurrentGroupId);
            Session["VerifiedUser"] = WebManager.GetUserDTO(imp.UserId).IsVerified;
            Session["ApprovedGroup"] = (WebManager.GetGroupDTO(imp.CurrentGroupId).IsApproved ?? false);
            Session["Ident"] = new GenericPrincipal(new GenericIdentity(imp.EmailAddress), new string[] { role });
        }
    }
}