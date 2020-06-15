using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Okurdostu.Web.Models;

namespace Okurdostu.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Route("~/gizlilik-politikasi")]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        [Route("~/kullanici-sozlesmesi")]
        public IActionResult UserAgreement()
        {
            return View();
        }
        [Route("~/sss")]
        public IActionResult FAQ()
        {
            return View();
        }
        [Route("~/kvkk")]
        public IActionResult KVKK()
        {
            return View();
        }
    }
}
