using System.Web.Optimization;

namespace FeesAutomationWebsite
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/datatable/jquery.dataTables.js",
                      "~/Scripts/dropzone/dropzone.min.js",
                        "~/Scripts/datatable-buttons/dataTables.buttons.js",
                         "~/Scripts/datatable-buttons/dataTables.buttons.min.js",
                         "~/Scripts/datatable-buttons/buttons.html5.min.js",
                         "~/Scripts/datatable-buttons/buttons.flash.min.js",
                         "~/Scripts/datatable-buttons/buttons.print.min.js",
                         "~/Scripts/datatable-buttons/buttons.colVis.min.js",
                         "~/Scripts/datatable-buttons/buttons.bootstrap4.min.js",
                         "~/Scripts/datatable-buttons/buttons.print.js",
                         "~/Scripts/notify/notify.min.js"
                      ));

            // Drop Zone
            bundles.Add(new ScriptBundle("~/script/dropzone").Include(
               "~/Scripts/dropzone/dropzone.js"));

            // KnockOut JS
            bundles.Add(new ScriptBundle("~/script/knockout").Include(
               "~/Scripts/knockout/knockout-3.4.2.js"));

            bundles.Add(new StyleBundle("~/styles/dropzone").Include(
                "~/Scripts/dropzone/dropzone.css"));
            // Date formatting lib
            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                        "~/Scripts/datatable/moment.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                            "~/Content/bootstrap.css",
                            "~/Content/site.css",
                            "~/Content/datatable/jquery.dataTables.min.css",
                            "~/Content/datatable-buttons/buttons.dataTables.min.css",
                            "~/Content/datatable-buttons/autoFill.dataTables.min.css",
                            "~/Content/datatable-buttons/font-awesome.min.css",
                            "~/Scripts/dropzone/dropzone.min.css"
                      ));

            // Scripts used on Pending Fee List page
            bundles.Add(new ScriptBundle("~/bundles/pendingfeelist").Include(
                "~/Scripts/PendingFeeList.js",
                "~/Scripts/PendingFeeProcessSelectedList.js"
                ));
            //Scripts used in Fee Type Mappings
            bundles.Add(new ScriptBundle("~/bundles/feetypemappingmanage").Include(
                "~/Scripts/FeeTypeMappingManage.js",
                "~/Scripts/FeeTypeMappingList.js",
                "~/Scripts/FeeTypeMappingProcessSelected.js"
                ));
            //Scripts used in Fee Setup 
            bundles.Add(new ScriptBundle("~/bundles/feemanage").Include(
                "~/Scripts/FeeManage.js",
                "~/Scripts/FeeList.js",
                "~/Scripts/FeeProcessSelected.js"
                ));
            //Scripts used in Notify 
            //bundles.Add(new ScriptBundle("~/bundles/notify").Include(
            //    "~/Scripts/notify/notify.min.js"
            //    ));
        }
    }
}
