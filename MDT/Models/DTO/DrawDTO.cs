using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models.DTO
{
    public class DrawDTO
    {
        public int DrawId { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int DrawTypeId { get; set; }
        public string DrawTypeName { get; set; }
        public string Results { get; set; }
        public string DrawCode { get; set; }
        public int MaxEntriesPerUser { get; set; }
        public int EntriesToDraw { get; set; }
        public bool RemoveDrawnEntries { get; set; }
        public bool RemoveDrawnUsers { get; set; }
        public int? NextDrawId { get; set; }
        public bool PassDrawnToNext { get; set; }
        public bool PassUndrawnToNext { get; set; }
        public bool AutoDraw { get; set; }
        public bool JoinConfirmationRequired { get; set; }
        public bool RefundConfirmationRequired { get; set; }

        public List<int> EntryIds { get; set; }
        public List<int> TransactionIds { get; set; }

        /// <summary>
        /// Create a DrawDTO from a Draw entity
        /// </summary>
        /// <param name="draw">Draw entity, must include DrawType, DrawOption, DrawEntries, and Transactions</param>
        public DrawDTO(Draw draw = null)
        {
            EntryIds = new List<int>();
            TransactionIds = new List<int>();

            if (draw != null)
            {
                DrawId = draw.DrawId;
                StartDateTime = draw.StartDateTime;
                EndDateTime = draw.EndDateTime;
                DrawTypeId = draw.DrawTypeId;
                DrawTypeName = draw.DrawType?.DrawTypeName;
                Results = draw.Results;
                DrawCode = draw.DrawCode;
                MaxEntriesPerUser = draw.DrawOption.MaxEntriesPerUser;
                EntriesToDraw = draw.DrawOption.EntriesToDraw;
                RemoveDrawnEntries = draw.DrawOption.RemoveDrawnEntries;
                RemoveDrawnUsers = draw.DrawOption.RemoveDrawnUsers;
                NextDrawId = draw.DrawOption.NextDrawId;
                PassDrawnToNext = draw.DrawOption.PassDrawnToNext;
                PassUndrawnToNext = draw.DrawOption.PassUndrawnToNext;
                AutoDraw = draw.DrawOption.AutoDraw;
                JoinConfirmationRequired = draw.DrawOption.JoinConfirmationRequired;
                RefundConfirmationRequired = draw.DrawOption.RefundConfirmationRequired;

                EntryIds = draw.DrawEntries.Select(de => de.EntryId).ToList();
                TransactionIds = draw.Transactions.Select(t => t.TransactionId).ToList();
            }
        }

    }
}