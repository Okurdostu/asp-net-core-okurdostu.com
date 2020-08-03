using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Okurdostu.Web.Models;

namespace Okurdostu.Web.Filters
{
    public class ConfirmedEmailFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            Controller controller = actionContext.Controller as Controller;

            if (controller.User.Identity.GetEmailConfirmStatus())
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                ReturnModel rm = new ReturnModel
                {
                    Succes = false,
                    Message = "Email adresinizi onaylamanız gerekli",
                    InternalMessage = "The email confirmation is required to do that."
                };

                actionContext.Result =  new BadRequestObjectResult(rm); 
            }
        }
    }
}
