using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.ViewModels
{
    public class ModalMessageVM
    {
        public string Header { get; set;}
        public string Body   { get; set;}
        public string Footer { get; set;}
        public bool HtmlHeader { get; set; }
        public bool HtmlBody { get; set; }
        public bool HtmlFooter { get; set; }
        public bool RedirectButton { get; set; }
        public string RedirectText { get; set; }
        public string RedirectLink { get; set; }
    }
}