using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDT.Models
{
    public class DdlItem
    {
        public int val { get; set; }
        public string txt { get; set; }
        public Dictionary<string, int> fltrs { get; set; }

        public DdlItem(int v, string t)
        {
            val = v;
            txt = t;
            fltrs = new Dictionary<string, int>();
        }

        public DdlItem(int v, string t, Dictionary<string, int> f)
        {
            val = v;
            txt = t;
            fltrs = f;
        }

        public DdlItem(int v, string t, string fnm, int f)
        {
            val = v;
            txt = t;
            fltrs = new Dictionary<string, int>() { { fnm, f } };

        }


    }
}