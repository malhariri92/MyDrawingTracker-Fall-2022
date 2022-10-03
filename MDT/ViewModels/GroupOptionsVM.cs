using MDT.Models;
using MDT.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MDT.ViewModels
{
    public class GroupOptionsVM
    {
        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        [Display(Name = "Join Confirmation Required")]
        public bool JoinConfirmation { get; set; }

        [Display(Name = "Access Code")]
        public string AccessCode { get; }
        public List<Description> Descriptions { get; set; }

        public GroupOptionsVM()
        {
            Descriptions = new List<Description>();
        }

        public GroupOptionsVM(Group group) : this()
        {
            GroupName = group.GroupName;
            JoinConfirmation = group.JoinConfirmationRequired;
            AccessCode = group.AccessCode;
        }

        public void SetDescriptions(List<Description> desc)
        {
            Descriptions = desc.OrderBy(ds => ds.SortOrder).ToList();
            if (!Descriptions.Any())
            {
                Descriptions.Add(new Description());
                Descriptions.Add(new Description());
                Descriptions.Add(new Description());
            }
        }
    }
}