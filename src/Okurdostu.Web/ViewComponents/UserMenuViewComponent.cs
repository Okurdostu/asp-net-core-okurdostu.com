using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Data.Model;
using Okurdostu.Web.Models;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "UserMenu")]
    public class UserMenuViewComponent : ViewComponent
    {
        
        private readonly OkurdostuContext Context;
        public UserMenuViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync()
        {
        
            if (User.Identity.IsAuthenticated)
            {
                var AuthUser = await Context.User.FirstOrDefaultAsync(x => x.Id.ToString() == User.Identity.GetUserId());
                return View(AuthUser);
            }
            return null;

        }

    }
}
