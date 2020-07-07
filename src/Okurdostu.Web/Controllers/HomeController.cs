using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class HomeController : BaseController<HomeController>
    {
        //viewbag.authuser i onactionexecuting e al.
        [Route("~/")]
        public async Task<IActionResult> Index()
        {
            ViewBag.AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            return View();
        }
        [Route("~/gizlilik-politikasi")]
        public async Task<IActionResult> PrivacyPolicy()
        {
            ViewBag.AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            return View();
        }
        [Route("~/kullanici-sozlesmesi")]
        public async Task<IActionResult> UserAgreement()
        {
            ViewBag.AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            return View();
        }
        [Route("~/sss")]
        public async Task<IActionResult> FAQ()
        {
            ViewBag.AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            return View();
        }
        [Route("~/kvkk")]
        public async Task<IActionResult> KVKK()
        {
            ViewBag.AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            return View();
        }
    }
}
