using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Okurdostu.Data.Model;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using Okurdostu.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class SignUpController : BaseController<SignUpController>
    {
        [Route("Kaydol")]
        public IActionResult Index(string ReturnUrl)
        {
            return HttpContext.User.Identity.IsAuthenticated ? (IActionResult)Redirect("/") : View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("Kaydol")]
        public async Task<IActionResult> Index(ProfileModel Model, string ReturnUrl)
        {
            if (blockedUsernames.Any(x => Model.Username == x))
            {
                TempData["SignUpMessage"] = "Bu kullanıcı adını: " + Model.Username + " kullanamazsınız";
                return Redirect("/kaydol");
            }

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            var User = new User
            {
                Username = Model.Username,
                Email = Model.Email,
                Password = Model.Password.SHA512(),
                FullName = Model.FullName,
            };

            int result = 0;
            try
            {
                await Context.User.AddAsync(User);
                result = await Context.SaveChangesAsync();
                Logger.LogInformation("{username}({userid}) signed up on {datetime}", User.Username, User.Id, DateTime.Now);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.Message.Contains("Unique_Key_Username"))
                    TempData["SignUpMessage"] = "Bu kullanıcı adını kullanamazsınız";
                else if (e.InnerException != null && e.InnerException.Message.Contains("Unique_Key_Email"))
                    TempData["SignUpMessage"] = "Bu e-mail adresini kullanamazsınız";
                else
                {
                    Logger.LogError("Guest taking a error when trying sign up Ex message: {ex.message}, InnerEx Message: {iex.message}", e?.Message, e?.InnerException?.Message);
                    TempData["SignUpMessage"] = "Başaramadık ve ne olduğunu bilmiyoruz";
                }
            }
            if (result > 0)
            {
                await DoAuth(User);

                var _UserEmailConfirmation = new UserEmailConfirmation()
                {
                    UserId = User.Id
                };
                await Context.AddAsync(_UserEmailConfirmation);
                await Context.SaveChangesAsync();

                var Email = new OkurdostuEmail((IEmailConfiguration)HttpContext?.RequestServices.GetService(typeof(IEmailConfiguration)))
                {
                    SenderMail = "halil@okurdostu.com",
                    SenderName = "Halil İbrahim Kocaöz"
                };

                Email.Send(Email.NewUserMail(User.FullName, User.Email, _UserEmailConfirmation.GUID));

                return string.IsNullOrEmpty(ReturnUrl) ? Redirect("/beta") : Redirect(ReturnUrl);
            }
            else if (TempData["SignUpMessage"] == null)
            {
                TempData["SignUpMessage"] = "Sorun yaşadık, kaydolmayı tekrar deneyiniz";
            }
            return View();
        }
    }
}
