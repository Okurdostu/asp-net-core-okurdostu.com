using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class LoginController : BaseController<LoginController>
    {
        [Route("Girisyap")]
#pragma warning disable IDE0060 // Remove unused parameter
        public IActionResult Index(string ReturnUrl)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return HttpContext.User.Identity.IsAuthenticated ? (IActionResult)Redirect("/") : View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("Girisyap")]
        public async Task<IActionResult> Index(LoginModel Model, string ReturnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            var User = await AuthenticateAsync(Model);
            if (User != null)
            {
                await SignInWithCookie(User);

                Logger.LogInformation(User.Username + " logged in at " + DateTime.Now);

                return string.IsNullOrEmpty(ReturnUrl) ? Redirect("/beta") : Redirect(ReturnUrl);
            }
            TempData["LoginMessage"] = "Kullanıcı adınız veya parolanız geçersiz";
            return View();
        }

        [NonAction]
        public async Task<User> AuthenticateAsync(LoginModel Model)
        {
            var User = await Context.User.Where(x => x.Username == Model.Username || x.Telephone == Model.Username || x.Email == Model.Username).FirstOrDefaultAsync();
            return User != null && User.PasswordCheck(Model.Password) ? User : null;
        }
    }
}
