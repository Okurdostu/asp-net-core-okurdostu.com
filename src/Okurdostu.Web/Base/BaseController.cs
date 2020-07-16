using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Data.Model;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Okurdostu.Web
{
    public class BaseController<T> : Controller where T : BaseController<T>
    {
        private ILogger<T> _logger;
        protected ILogger<T> Logger => _logger ?? (_logger = HttpContext.RequestServices.GetService<ILogger<T>>());
        public OkurdostuContext Context => (OkurdostuContext)HttpContext?.RequestServices.GetService(typeof(OkurdostuContext));


        public string[] blockedUsernames = {
            "","comment","account","confirmemail","girisyap","kaydol","ihtiyaclar","beta","arama",
            "gizlilik-politikasi","kullanici-sozlesmesi","sss","kvkk",
            "home", "like","logout", "ihtiyac-olustur","ihtiyac", "universiteler"
        };

        public async Task SignInWithCookie(User user)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var ClaimList = new List<Claim>();
            ClaimList.Add(new Claim("Id", user.Id.ToString()));
            ClaimList.Add(new Claim("Username", user.Username));
            ClaimList.Add(new Claim("Email", user.Email));
            ClaimList.Add(new Claim("EmailState", user.IsEmailConfirmed.ToString()));
            if (user.PictureUrl != null)
            {
                ClaimList.Add(new Claim("Photo", user.PictureUrl));
            }
            else
            {
                ClaimList.Add(new Claim("Photo", ""));
            }

            var ClaimsIdentity = new ClaimsIdentity(ClaimList, CookieAuthenticationDefaults.AuthenticationScheme);
            var AuthProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(ClaimsIdentity),
                AuthProperties);
        }

        public async Task<User> GetAuthenticatedUserFromDatabaseAsync()
        {
            var Id = User?.Identity?.GetUserId();
            if (Id != null)
            {
                var _User = await Context.User.FirstOrDefaultAsync(x => x.Id == long.Parse(Id) && x.IsActive);
                return _User != null ? _User : null;
            }
            else
            {
                return null;
            }
        }
    }
}
