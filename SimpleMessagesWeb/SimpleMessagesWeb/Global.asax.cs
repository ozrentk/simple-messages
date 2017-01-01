using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebDemo.App_Start;

namespace WebDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            log.Info("Log configured");

            AreaRegistration.RegisterAllAreas();
            log.Info("Areas registered");

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            log.Info("Global filters registered");

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            log.Info("Routes registered");

            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log.Info("Bundles registered");
        }
    }
}
