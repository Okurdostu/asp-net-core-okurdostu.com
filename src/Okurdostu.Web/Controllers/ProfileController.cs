using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Web.Base;

namespace Okurdostu.Web.Controllers
{
    public class ProfileController : OkurdostuContextController
    {
        public async Task<IActionResult> Index(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var User = await Context.User.FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
                if (User != null) return View(User);
            }

            //404
            return null;
        }

    }
}
