using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using Okurdostu.Web.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class SignUpController : BaseController<SignUpController>
    {
        [Route("Kaydol")]
#pragma warning disable IDE0060 // Remove unused parameter
        public IActionResult Index(string ReturnUrl)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return HttpContext.User.Identity.IsAuthenticated ? (IActionResult)Redirect("/") : View();
        }

        public class SignUpModel
        {
            [Required(ErrorMessage = "Kullanıcı adınızı yazmalısınız")]
            [Display(Name = "Kullanıcı adı")]
            [MaxLength(15, ErrorMessage = "Çok uzun, en fazla 15 karakter")]
            [MinLength(3, ErrorMessage = "Çok kısa")]
            [RegularExpression(@"[0-9a-z]+", ErrorMessage = "Sadece küçük harflerle latin karakterler ve rakamlar")]
            public string Username { get; set; }

            [Required(ErrorMessage = "E-mail adresinizi yazmalısınız")]
            [Display(Name = "Email adresi")]
            [MaxLength(40, ErrorMessage = "Çok uzun, en fazla 40 karakter")]
            [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Lütfen geçerli bir e-mail adresi olsun")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Parola seçmelisiniz")]
            [Display(Name = "Parola")]
            [DataType(DataType.Password)]
            [MinLength(7, ErrorMessage = "En az 7 karakterden oluşan bir şifre oluşturun")]
            [MaxLength(30, ErrorMessage = "Çok uzun, en fazla 30 karakter")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Adınızı ve soyadınızı yazmalısınız")]
            [Display(Name = "Adınız ve soyadınız")]
            [MaxLength(50, ErrorMessage = "Çok uzun, en fazla 50 karakter")]
            [MinLength(5, ErrorMessage = "Çok kısa")]
            [RegularExpression(@"[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+", ErrorMessage = "Gerçekten adınız ve soyadınız da harf dışında karakter mı var?")]
            public string FullName { get; set; }
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("Kaydol")]
        public async Task<IActionResult> Index(SignUpModel Model, string ReturnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            if (!ModelState.IsValid)
            {
                TempData["SignUpMessage"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage;
                return Redirect("/kaydol");
            }
            if (blockedUsernames.Any(x => Model.Username == x))
            {
                TempData["SignUpMessage"] = "Bu kullanıcı adını: " + Model.Username + " kullanamazsınız";
                return Redirect("/kaydol");
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
                {
                    TempData["SignUpMessage"] = "Bu kullanıcı adını kullanamazsınız";
                }
                else if (e.InnerException != null && e.InnerException.Message.Contains("Unique_Key_Email"))
                {
                    TempData["SignUpMessage"] = "Bu e-mail adresini kullanamazsınız";
                }
                else
                {
                    Logger.LogError("Guest taking a error when trying sign up Ex message: {ex.message}, InnerEx Message: {iex.message}", e?.Message, e?.InnerException?.Message);
                    TempData["SignUpMessage"] = "Başaramadık ve ne olduğunu bilmiyoruz";
                }
            }
            if (result > 0)
            {
                await SignInWithCookie(User);

                var _UserEmailConfirmation = new UserEmailConfirmation
                {
                    UserId = User.Id
                };
                await Context.AddAsync(_UserEmailConfirmation);
                await Context.SaveChangesAsync();

                var Email = new OkurdostuEmail((IEmailConfigurationService)HttpContext?.RequestServices.GetService(typeof(IEmailConfigurationService)))
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
