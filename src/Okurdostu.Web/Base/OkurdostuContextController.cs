using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;

namespace Okurdostu.Web.Base
{
    public class OkurdostuContextController : Controller
    {
        private OkurdostuContext _con;
        public OkurdostuContext Context => _con ?? (OkurdostuContext)HttpContext?.RequestServices.GetService(typeof(OkurdostuContext));
    }
}
