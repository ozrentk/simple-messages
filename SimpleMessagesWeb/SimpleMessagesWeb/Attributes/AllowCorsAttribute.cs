using System.Web.Mvc;

namespace WebDemo.Attributes
{
    public class AllowCorsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ctx = filterContext.RequestContext.HttpContext;

            ctx.Response.AddHeader("Access-Control-Allow-Origin", "*"); // TODO: "http://localhost" ???

            base.OnActionExecuting(filterContext);
        }
    }
}