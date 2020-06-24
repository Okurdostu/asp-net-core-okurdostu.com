using Microsoft.AspNetCore.Mvc;

namespace Okurdostu.Web.Controllers
{
    public class SearchController : BaseController<SearchController>
    {
        [Route("~/arama")]
        [Route("~/arama/{q}")]
        public IActionResult Index(string q)
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
