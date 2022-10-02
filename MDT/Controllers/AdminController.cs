using MDT.Filters;
using MDT.Models;
using MDT.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
                                      .ToList()
                                      .Select(g => new GroupVM(g))
                                      .ToList();

            foreach (GroupVM gvm in vm)
            {
                gvm.SetDescriptions(db.Descriptions.Where(d => new List<int>() { 1, 5, 6 }.Contains(d.ObjectTypeId) && d.ObjectId == gvm.GroupId).ToList());
            }


            return View(vm);
        }

        public async Task<ActionResult> Approved(int id)
        {
            Group g = await GetGroup(id);
            if (g == null)
            {
                TempData["Error"] = $"Group id {id} not found";
            }

            if (g.IsApproved != null)
            {
                TempData["Error"] = $"Group: {g.GroupName} has already been {(g.IsApproved.Value ? "approved" : "rejected")}";
            }

            g.IsApproved = true;
            db.Entry(g).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["Message"] = $"Group: {g.GroupName} has been {(g.IsApproved.Value ? "approved" : "rejected")}";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Reject(int id)
        {
            GroupVM vm = new GroupVM(await GetGroup(id));
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Rejected(GroupVM vm)
        {
            Group g = await GetGroup(vm.GroupId);
            if (g == null)
            {
                TempData["Error"] = $"Group id {vm.GroupId} not found";
                return RedirectToAction("Index");
            }

            if (g.IsApproved != null)
            {
                TempData["Error"] = $"Group:  {g.GroupName} has already been {(g.IsApproved.Value ? "approved" : "rejected")}";
                return RedirectToAction("Index");
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
            await db.SaveChangesAsync();

            TempData["Message"] = $"Group: {g.GroupName} has been {(g.IsApproved.Value ? "approved" : "rejected")}";
            return RedirectToAction("Index");
        }
    }
}