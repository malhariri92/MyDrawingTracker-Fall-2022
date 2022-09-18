using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models.DTO
{
    public class DrawEntryDTO
    {
        public int EntryId { get; set; }
        public int DrawId { get; set; }
        public DateTime? DrawStartDate { get; set; }
        public DateTime DrawEndDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EntryCode { get; set; }

        /// <summary>
        /// Create DrawEntryDTO from a DrawEntry entity
        /// </summary>
        /// <param name="entry">DrawEntry entity, must include Draw and User</param>
        public DrawEntryDTO(DrawEntry entry = null)
        {
            if (entry != null)
            {
                EntryId = entry.EntryId;
                DrawId = entry.DrawId;
                DrawStartDate = entry.Draw.StartDateTime;
                DrawEndDate = entry.Draw.EndDateTime;
                UserId = entry.UserId;
                UserName = entry.User.UserName;
                EntryCode = entry.EntryCode;
            }
        }
    }
}