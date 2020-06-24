using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Okurdostu.Data.Model;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class SignUpController : BaseController<SignUpController>
    {
        [Route("~/Kaydol")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/Kaydol")]
        public async Task<IActionResult> Index(ProfileModel Model)
        {
            var User = new User
            {
                Username = Model.Username,
                Email = Model.Email,
                Password = Model.Password.SHA512(),
                FullName = Model.FullName,
            };
            try
            {
                await Context.User.AddAsync(User);
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    var ClaimList = new List<Claim>();
                    ClaimList.Add(new Claim("Id", User.Id.ToString()));
                    ClaimList.Add(new Claim("Username", User.Username));
                    ClaimList.Add(new Claim("Email", User.Email));
                    ClaimList.Add(new Claim("FullName", User.FullName));
                    var ClaimsIdentity = new ClaimsIdentity(ClaimList, CookieAuthenticationDefaults.AuthenticationScheme);
                    var AuthProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = true

                    };
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(ClaimsIdentity),
                        AuthProperties);

                    Logger.LogInformation(User.Username + " signed up at " + DateTime.Now);

                    return Redirect("/beta");
                }
                else
                    TempData["SignUpMessage"] = "Başaramadık ve ne olduğunu bilmiyoruz";
            }
            catch (Exception e)
            {
                if (e.InnerException.Message.Contains("Unique_Key_Username"))
                    TempData["SignUpMessage"] = "Bu kullanıcı adını kullanamazsınız";
                else if (e.InnerException.Message.Contains("Unique_Key_Email"))
                    TempData["SignUpMessage"] = "Bu e-mail adresini kullanamazsınız";
                else
                {
                    Logger.LogError("Guest taking a error when trying sign up Ex message: {ex.message}, InnerEx Message: {iex.message}", e.Message, e.InnerException.Message);
                    TempData["SignUpMessage"] = "Başaramadık ve ne olduğunu bilmiyoruz";
                }
            }
            return View();
        }
    }
}
