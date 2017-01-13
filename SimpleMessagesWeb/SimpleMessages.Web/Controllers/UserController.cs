using System.Web.Mvc;
using SimpleMessages.Web.Attributes;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using SimpleMessages.Web.Models;
using SimpleMessages.Identity;
using System;

namespace SimpleMessages.Web.Controllers
{
    [AllowCors]
    public class UserController : Controller
    {
        private RoleManager _roleManager;
        private UserManager _userManager;

        public SimpleMessages.Identity.RoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<RoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public SimpleMessages.Identity.UserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().Get<UserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [Route("")]
        [Route("Chat")]
        [Authorize]
        public ActionResult Index()
        {
            var model = new Models.Chat.Index
            {
                CurrentUser = User.Identity.Name
            };

            return View("Index", model);
        }

        [Route("Chat2")]
        [Authorize]
        public ActionResult Index2()
        {
            var model = new Models.Chat.Index
            {
                CurrentUser = User.Identity.Name
            };

            return View("Index-ko", model);
        }
    }
}