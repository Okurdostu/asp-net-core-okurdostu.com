using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Okurdostu.Web.Filters
{
    public class ConfirmedEmailFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            Controller controller = actionContext.Controller as Controller;

            if (controller.User.Identity.GetEmailConfirmedState())
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                controller.TempData["ProfileMessage"] = "Bu işlemi yapabilmeniz için e-mail adresinizi onaylamalısınız" +
                    "<br>" +
                    "<a class='od' href='/beta'>Daha fazla bilgi için tıklayınız</a>";
                actionContext.Result = new RedirectResult("/" + controller.User.Identity.GetUsername(), false);
            }
        }
    }
}
