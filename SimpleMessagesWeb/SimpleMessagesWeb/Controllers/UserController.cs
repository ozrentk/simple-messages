using System.Web.Mvc;
using WebDemo.Attributes;

namespace WebDemo.Controllers
{
    [AllowCors]
    public class UserController : Controller
    {
        [Route("")]
        [Route("Chat")]
        public ActionResult Index()
        {
            return View();
        }
    }
}