using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDemo.Controllers
{
    public class PublicController : Controller
    {
        [Route("Public")]
        public ActionResult Index()
        {
            return View();
        }

        //[Route("test123/{value?}")]
        //public ActionResult Show(int? value)
        //{
        //    return View();
        //}
    }
}