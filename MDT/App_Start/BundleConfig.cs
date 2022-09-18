using System.Web;
using System.Web.Optimization;

namespace MDT
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap-toggle.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/themes/base/jquery-ui.css",
                "~/Content/bootstrap.css",
                "~/Content/fontawesome.css",
                "~/Content/Site.css",
                "~/Content/bootstrap-toggle.less"));
        }
    }
}
