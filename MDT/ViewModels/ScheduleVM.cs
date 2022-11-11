using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MDT.Models;

namespace MDT.ViewModels
{
    public class ScheduleVM
    {
        public List<ScheduleDayVM> Days { get; set; }

        public ScheduleVM()
        {
            Days = new List<ScheduleDayVM>();
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek d = (DayOfWeek)i;
                Days.Add(new ScheduleDayVM()
                {
                    DayName = d.ToString(),
                    Abbr = d.ToString().Substring(0, 3),
                    DayNumber = i
                });
            }
        }

        public ScheduleVM(List<Schedule> sched) : this()
        {
            foreach(Schedule s in sched )
            {
                ScheduleDayVM vm = Days.Find(d => d.DayNumber == s.DayOfWeek);
                vm.Active = true;
                vm.DrawTime = s.Time;
            }
        }

    }

    public class ScheduleDayVM
    {
        public string DayName { get; set; }
        public string Abbr { get; set; }
        public int DayNumber { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan? DrawTime { get; set; }
        public bool Active { get; set; }
    }
}