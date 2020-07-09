using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Data.Model;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

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
