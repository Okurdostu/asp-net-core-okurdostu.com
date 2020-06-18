
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
                var _User = await Context.User.
                    Include(user => user.UserEducation).
                    ThenInclude(x => x.University).
                    FirstOrDefaultAsync(x => x.Username == username && x.IsActive);

                if (_User != null) return View(_User);
            }

            //404
            return null;
        }

    }
}
