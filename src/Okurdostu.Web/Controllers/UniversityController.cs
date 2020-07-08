using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class UniversityController : BaseController<UniversityController>
    {
        [Route("universiteler")]
        public IActionResult Index()
        {
            ViewData["UniversityActiveClass"] = "active";
            return View(Context.University.ToList());
        }
        [Route("universite/{friendlyname}")]
        public async Task<IActionResult> UniversityPage(string friendlyname)
        {
            ViewData["UniversityActiveClass"] = "active";
            var University = await Context.University.FirstOrDefaultAsync(x => x.FriendlyName == friendlyname);
            return View(University);
        }


    }
}
