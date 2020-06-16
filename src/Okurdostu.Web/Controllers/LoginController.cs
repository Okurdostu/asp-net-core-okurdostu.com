using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Okurdostu.Web.Models;

namespace Okurdostu.Web.Controllers
{
    public class LoginController : Controller
    {
        [Route("~/Girisyap")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
