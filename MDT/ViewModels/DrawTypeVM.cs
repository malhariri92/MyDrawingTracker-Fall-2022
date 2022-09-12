using MDT.Models;
using System.Collections.Generic;
using System.Linq;

namespace MDT.ViewModels
{
    public class DrawTypeVM
    {
        public int DrawTypeId { get; set; }
        public string GameName { get; set; }
        public decimal EntryCost { get; set; }
        public List<string> Users { get; set; }
        public ScheduleVM Schedule { get; set; }
        public bool IsActive { get; set; }

        public DrawTypeVM() { }
        public DrawTypeVM(DrawType g) 
        {
            if (g != null)
            {
                DrawTypeId = g.DrawTypeId;
                GameName = g.DrawTypeName;
                EntryCost = g.EntryCost;
                Users = g.UserDrawTypeOptions.Where(p => p.UserId != 0).OrderBy(p => p.User.UserName).Select(p => p.User.UserName).ToList();
                Schedule = new ScheduleVM(g.Schedules.ToList());
                IsActive = g.IsActive;
            }
        }
    }
}