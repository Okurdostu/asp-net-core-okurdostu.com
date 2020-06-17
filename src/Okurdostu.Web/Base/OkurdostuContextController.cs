using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Data.Model;
using System.Threading.Tasks;

namespace Okurdostu.Web.Base
{
    public class OkurdostuContextController : Controller
    {
        private OkurdostuContext _con;
        public OkurdostuContext Context => _con ?? (OkurdostuContext)HttpContext?.RequestServices.GetService(typeof(OkurdostuContext));

        //başka bir controllera alınacak
        public async Task<User> GetAuthenticatedUserFromDatabase()
        {
            var Username = User.Identity.GetUsername();
            var _User = await Context.User.FirstOrDefaultAsync(x => x.Username == Username);
            return _User != null ? _User : null;
        }
    }
}
