using Microsoft.AspNetCore.Mvc;

namespace Okurdostu.Web.Controllers
{
    public class HomeController : Controller
    {
        //viewbag.authuser i onactionexecuting e al.
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("gizlilik-politikasi")]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        [Route("kullanici-sozlesmesi")]
        public IActionResult UserAgreement()
        {
            return View();
        }
        [Route("sss")]
        public IActionResult FAQ()
        {
            return View();
        }
        [Route("kvkk")]
        public IActionResult KVKK()
        {
            return View();
        }
    }
}
