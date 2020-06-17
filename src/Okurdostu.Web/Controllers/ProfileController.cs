using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;

namespace Okurdostu.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly OkurdostuContext Context;
        public ProfileController(OkurdostuContext _context) => Context = _context;

        public async Task<IActionResult> Index(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var User = await Context.User.FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
                if (User != null) return View(User);
            }

            //404
            return null;
        }
    }
}
