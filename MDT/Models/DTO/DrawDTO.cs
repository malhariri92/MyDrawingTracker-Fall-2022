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
        public string Title { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Create a DrawDTO from a Draw entity
        /// </summary>
        /// <param name="draw">Draw entity, must include DrawType</param>
        public DrawDTO(Draw draw = null)
        {
          
            if (draw != null)
            {
                DrawId = draw.DrawId;
                StartDateTime = draw.StartDateTime;
                EndDateTime = draw.EndDateTime;
                DrawTypeId = draw.DrawTypeId;
                DrawTypeName = draw.DrawType?.DrawTypeName;
                Results = draw.Results;
                DrawCode = draw.DrawCode;
                Title = draw.Title ?? $"{draw.DrawType.DrawTypeName} {draw.EndDateTime:yyyy-MM-dd}";
                IsActive = draw.StartDateTime != null && draw.EndDateTime > DateTime.Now && draw.Results == null;


            }
        }

    }
}