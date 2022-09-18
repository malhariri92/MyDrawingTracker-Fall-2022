using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models.DTO
{
    public class DrawTypeDTO
    {
        public int DrawTypeId { get; set; }
        public string DrawTypeName { get; set; }
        public decimal EntryCost { get; set; }
        public bool IsActive { get; set; }
        public bool IsInternal { get; set; }
        public int EntriesToDraw { get; set; }
        public int MaxEntriesPerUser { get; set; }
        public bool RemoveDrawnEntries { get; set; }
        public bool RemoveDrawnUsers { get; set; }
        public int NumberOfDraws { get; set; }
        public bool PassDrawnToNext { get; set; }
        public bool PassUndrawnToNext { get; set; }
        public bool AutoDraw { get; set; }
        public bool JoinConfirmationRequired { get; set; }
        public bool RefundConfirmationRequired { get; set; }
        public decimal InitialUserBalance { get; set; }
        public List<int> DrawIds { get; set; }

        /// <summary>
        /// Create DrawTypeDTO from a DrawType entity
        /// </summary>
        /// <param name="drawType">DrawType entity, must include Draws</param>
        public DrawTypeDTO(DrawType drawType = null)
        {
            DrawIds = new List<int>();
            if (drawType != null)
            {
                DrawTypeId = drawType.DrawTypeId;
                DrawTypeName = drawType.DrawTypeName;
                EntryCost = drawType.EntryCost;
                IsActive = drawType.IsActive;
                IsInternal = drawType.IsInternal;
                EntriesToDraw = drawType.EntriesToDraw;
                MaxEntriesPerUser = drawType.MaxEntriesPerUser;
                RemoveDrawnEntries = drawType.RemoveDrawnEntries;
                RemoveDrawnUsers = drawType.RemoveDrawnUsers;
                NumberOfDraws = drawType.NumberOfDraws;
                PassDrawnToNext = drawType.PassDrawnToNext;
                PassUndrawnToNext = drawType.PassUndrawnToNext;
                AutoDraw = drawType.AutoDraw;
                JoinConfirmationRequired = drawType.JoinConfirmationRequired;
                RefundConfirmationRequired = drawType.RefundConfirmationRequired;
                InitialUserBalance = drawType.InitialUserBalance;
                DrawIds = drawType.Draws.Select(d => d.DrawId).ToList();
            }
        }
    }
}