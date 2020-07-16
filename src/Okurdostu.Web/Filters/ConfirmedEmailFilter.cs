using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Okurdostu.Data;
using System.Linq;

namespace Okurdostu.Web.Filters
{
    public class ConfirmedEmailFilter : ActionFilterAttribute
    {
        private readonly OkurdostuContext dbContext;
        public ConfirmedEmailFilter(OkurdostuContext _context) => dbContext = _context;

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
