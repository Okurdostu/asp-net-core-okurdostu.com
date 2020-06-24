using Microsoft.AspNetCore.Mvc;
using Okurdostu.Web.Base;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class SearchController : OkurdostuContextController<SearchController>
    {
        [Route("~/arama")]
        [Route("~/arama/{q}")]
        public async Task<IActionResult> Index(string q)
        {
            
            if (!string.IsNullOrEmpty(q) && !string.IsNullOrWhiteSpace(q))
            {
                TempData["SearchPageTitle"] = q + " arama sonuçları | Okurdostu";
                TempData["q"] = q;
            }
            else
                TempData["SearchPageTitle"] = "Arama, keşfetme | Okurdostu";

            ViewData["SearchActiveClass"] = "active";
            return View();

        }
    }
}
