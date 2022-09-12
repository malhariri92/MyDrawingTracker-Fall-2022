using MDT.Models;
using System;

namespace MDT.ViewModels
{
    public class EntryVM
    {
        public int DrawId { get; set; }
        public DateTime DrawDate { get; set; }
        public int DrawTypeId { get; set; }
        public string GameName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int EntryCount { get; set; }

        public EntryVM()
        { }
        
        public EntryVM(Entry e)
        {
            DrawId = e.DrawId;
            DrawDate = e.Draw.EndDateTime;
            DrawTypeId = e.Draw.DrawTypeId;
            GameName = e.Draw.DrawType.DrawTypeName;
            UserId = e.UserId;
            UserName = e.User.UserName;
            EntryCount = 1;
        }
    }
}