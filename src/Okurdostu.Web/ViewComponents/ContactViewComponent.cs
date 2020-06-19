using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "Contact")]
    public class ContactViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public ContactViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var AuthUser = await Context.User.FirstOrDefaultAsync(x => x.Id.ToString() == User.Identity.GetUserId());
                var Model = new ProfileModel
                {
                    ContactEmail = AuthUser.ContactEmail,
                    Github = AuthUser.Github,
                    Twitter = AuthUser.Twitter
                };
                return View(Model);
            }
            return null;
        }
    }
}
