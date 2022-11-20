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
        public bool JoinConfirmationRequired { get; set; }
        public bool RefundConfirmationRequired { get; set; }
        public decimal InitialUserBalance { get; set; }
        public List<DrawDTO> Draws { get; set; }

        /// <summary>
        /// Create DrawTypeDTO from a DrawType entity
        /// </summary>
        /// <param name="drawType">DrawType entity, must include Draws</param>
        public DrawTypeDTO(DrawType drawType = null)
        {
            Draws = new List<DrawDTO>();
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
                JoinConfirmationRequired = drawType.JoinConfirmationRequired;
                RefundConfirmationRequired = drawType.RefundConfirmationRequired;
                InitialUserBalance = drawType.InitialUserBalance;
                Draws = drawType.Draws.Select(d => new DrawDTO(d)).ToList();
            }
        }
    }
}