using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MDT.ViewModels;
using MDT.Models;
using System.Data.Entity;
using MDT.Models.DTO;
using MDT.Filters;

namespace MDT.Controllers
{
    public class GroupController : BaseController
    {

        public ActionResult Index()
        {
            GroupOptionsVM vm = new GroupOptionsVM(db.Groups.Where(g => group.GroupId == g.GroupId).FirstOrDefault());
            vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectId == group.GroupId && d.ObjectTypeId == 1).ToList());
            return View(vm);
        }

        [AdminFilter(Role = "Admin")]
        public ActionResult Edit()
        {
            GroupOptionsVM vm = new GroupOptionsVM(db.Groups.Where(g => group.GroupId == g.GroupId).FirstOrDefault());
            vm.SetDescriptions(db.Descriptions.Where(d => d.ObjectId == group.GroupId && d.ObjectTypeId == 1).ToList());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GroupOptionsVM vm)
        {
            if (ModelState.IsValid)
            {
                Group grp = db.Groups.Find(group.GroupId);
                grp.GroupName = vm.GroupName;
                db.Entry(grp).State = EntityState.Modified;
                Description d1 = db.Descriptions.Find(1, group.GroupId, 1) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 1, IsNew = true };
                d1.Title = vm.Descriptions[0].Title;
                d1.TextBody = vm.Descriptions[0].TextBody;
                db.Entry(d1).State = d1.IsNew ? EntityState.Added : EntityState.Modified;
                Description d2 = db.Descriptions.Find(1, group.GroupId, 2) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 2, IsNew = true };
                d2.Title = vm.Descriptions[1].Title;
                d2.TextBody = vm.Descriptions[1].TextBody;
                db.Entry(d2).State = d2.IsNew ? EntityState.Added : EntityState.Modified;
                Description d3 = db.Descriptions.Find(1, group.GroupId, 3) ?? new Description() { ObjectTypeId = 1, ObjectId = group.GroupId, SortOrder = 3, IsNew = true };
                d3.Title = vm.Descriptions[2].Title;
                d3.TextBody = vm.Descriptions[2].TextBody;
                db.Entry(d3).State = d3.IsNew ? EntityState.Added : EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [AdminFilter(Role = "Admin")]
        public void JoinConfirmation(bool flag)
        {
            Group grp = db.Groups.Find(group.GroupId);

            grp.JoinConfirmationRequired = flag;
            db.Entry(grp).State = EntityState.Modified;
            db.SaveChanges();

        }
    }
}