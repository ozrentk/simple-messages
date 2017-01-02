using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDemo.Controllers
{
    public class AdminController : Controller
    {
        [Route("Admin")]
        public ActionResult Index()
        {
            return View();
        }
    }
}