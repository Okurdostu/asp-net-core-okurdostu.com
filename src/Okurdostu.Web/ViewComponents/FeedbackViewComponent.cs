using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Models;

using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name ="Feedback")]
    public class FeedbackViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public FeedbackViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync()
        {

            if (User.Identity.IsAuthenticated)
            {

                var AuthUser = await Context.User.FirstOrDefaultAsync(x => x.Id.ToString() == User.Identity.GetUserId());
                var Model = new FeedbackModel
                {
                    Email = AuthUser.Email,
                };
                return View(Model);

            }

            return View();
        }
    }
}
