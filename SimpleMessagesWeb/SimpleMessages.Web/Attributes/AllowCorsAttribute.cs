using System.Web.Mvc;

namespace SimpleMessages.Web.Attributes
{
    public class AllowCorsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ctx = filterContext.RequestContext.HttpContext;

            // TODO: instead of asterisk, could use configured URL for CORS 
            ctx.Response.AddHeader("Access-Control-Allow-Origin", "*"); 

            base.OnActionExecuting(filterContext);
        }
    }
}