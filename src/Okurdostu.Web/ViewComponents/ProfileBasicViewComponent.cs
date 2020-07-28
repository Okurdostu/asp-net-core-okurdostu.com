using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "ProfileBasic")]
    public class ProfileBasicComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public ProfileBasicComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var AuthUser = await Context.User.FirstOrDefaultAsync(x => x.Id.ToString() == User.Identity.GetUserId());
                var Model = new Web.Controllers.Api.MeController.ProfileModel
                {
                    Biography = AuthUser.Biography,
                    FullName = AuthUser.FullName
                };
                return View(Model);
            }
            return null;
        }
    }
}
