using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace SimpleMessages.Web.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/require").Include(
                        "~/Scripts/require.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryvalidate").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/bootstrap-switch.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/cookie").Include(
                        "~/Scripts/js-cookie/js.cookie.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css",
                        "~/Content/Site.css"));

            // TODO: this can be done -> investigate
            //bundles.Add(new StyleBundle("~/Content/logincss").Include(
            //  "~/Content/bootstrap.css",
            //  "~/Content/login.css"));

        }
    }
}