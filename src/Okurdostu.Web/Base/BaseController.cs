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


        private OkurdostuContext _con;
        public OkurdostuContext Context => _con ?? (OkurdostuContext)HttpContext?.RequestServices.GetService(typeof(OkurdostuContext));

        [Authorize]
        public async Task<User> GetAuthenticatedUserFromDatabaseAsync()
        {
            var Id = User.Identity.GetUserId();
            if (Id == null)
                return null;

            var _User = await Context.User.FirstOrDefaultAsync(x => x.Id == long.Parse(Id) && x.IsActive);
            return _User != null ? _User : null;
        }
    }
}
