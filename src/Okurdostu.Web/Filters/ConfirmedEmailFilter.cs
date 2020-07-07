using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System.Linq;

namespace Okurdostu.Web.Filters
{
    public class ConfirmedEmailFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Controller controller = context.Controller as Controller;

            long AuthUserId = long.Parse(context.HttpContext.User.Identity.GetUserId());
            using (var dbContext = new OkurdostuContext())
            {
                var User = dbContext.User.FirstOrDefault(x => x.Id == AuthUserId);

                if (User.IsEmailConfirmed)
                {
                    base.OnActionExecuting(context);
                }
                else
                {
                    controller.TempData["ProfileMessage"] = "Bu işlemi yapabilmeniz için e-mail adresinizi onaylamalısınız" +
                        "<br>" +
                        "<a class='od' href='/beta'>Daha fazla bilgi için tıklayınız</a>";
                    context.Result = new RedirectResult("/" + User.Username, false);
                }
            };
        }
    }
}
