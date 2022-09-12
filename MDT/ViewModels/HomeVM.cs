using MDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class HomeVM
    {
        public List<DrawTypeVM> Games { get; set; }
        public List<DrawVM> Draws { get; set; }
        public List<UserVM> Users { get; set; }

        public HomeVM()
        {

        }

        public HomeVM(DbEntities db)
        {
            Games = db.DrawTypes.Where(a => a.IsActive).ToList().Select(a => new DrawTypeVM(a)).ToList();
            Draws = db.Draws.Where(a => a.EndDateTime > DateTime.Now).ToList().Select(a => new DrawVM(a)).ToList();
            Users = db.Users.ToList().Select(a => new UserVM(a)).ToList();
        }
    }
}