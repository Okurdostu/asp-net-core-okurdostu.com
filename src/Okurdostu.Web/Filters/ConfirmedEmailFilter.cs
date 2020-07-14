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
        private readonly OkurdostuContext Context;
        public ConfirmedEmailFilter(OkurdostuContext _context) => Context = _context;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Controller controller = context.Controller as Controller;

            long AuthUserId = long.Parse(context.HttpContext.User.Identity.GetUserId());
            var User = Context.User.FirstOrDefault(x => x.Id == AuthUserId);

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
        }
    }
}
