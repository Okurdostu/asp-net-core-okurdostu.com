using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Okurdostu.Data.Model;
using Okurdostu.Data.Model.Context;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;

namespace Okurdostu.Web.Controllers
{
    public class LoginController : Controller
    {
        [Route("~/Girisyap")]
        public IActionResult Index(string ReturnUrl)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                return View();
            else if (ReturnUrl != null)
                return Redirect(ReturnUrl);
            else
                return Redirect("/");
        }
        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/Girisyap")]
        public async Task<IActionResult> Index(LoginModel Model, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var User = await AuthenticateAsync(Model);
                if (User != null)
                {
                    var ClaimList = new List<Claim>();
                    ClaimList.Add(new Claim("Id", User.Id.ToString()));
                    ClaimList.Add(new Claim("Username", User.Username));
                    ClaimList.Add(new Claim("Email", User.Username));
                    ClaimList.Add(new Claim("FullName", User.Username));
                    if (User.PictureUrl != null)
                        ClaimList.Add(new Claim("PictureUrl", User.PictureUrl.ToString()));

                    var ClaimsIdentity = new ClaimsIdentity(ClaimList, CookieAuthenticationDefaults.AuthenticationScheme);

                    var AuthProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(ClaimsIdentity),
                        AuthProperties);

                    return ReturnUrl != null ? Redirect(ReturnUrl) : Redirect("/beta");
                }
                TempData["LoginMessage"] = "Kullanıcı adınız veya parolanız geçersiz";
            }
            return View();
        }
        private readonly OkurdostuContext Context;
        public LoginController(OkurdostuContext _context) => Context = _context;
        public async Task<User> AuthenticateAsync(LoginModel Model)
        {
            var User = await Context.User.Where(x => x.Username == Model.Username || x.Telephone == Model.Username || x.Email == Model.Username).FirstOrDefaultAsync();
            return User != null && User.Password == Model.Password.SHA512() ? User : null;
        }
    }
}
