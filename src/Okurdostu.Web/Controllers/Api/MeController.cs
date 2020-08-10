using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using Okurdostu.Web.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    [ServiceFilter(typeof(ConfirmedEmailFilter))]
    public class MeController : SecureApiController
    {
        private readonly ILocalStorageService LocalStorage;
        private readonly IHostingEnvironment Environment;

        public MeController(IHostingEnvironment hostingEnvironment, ILocalStorageService localStorageService)
        {
            LocalStorage = localStorageService;
            Environment = hostingEnvironment;
        }

        [HttpGet("")]
        public ActionResult Index()
        {
            return NotFound();
        }

        public class UsernameModel
        {
            [Required(ErrorMessage = "Kimliğinizi doğrulamak için şuan ki parolanızı girmelisiniz")]
            [Display(Name = "Onay parolası")]
            [DataType(DataType.Password)]
            public string UsernameConfirmPassword { get; set; }

            [Required(ErrorMessage = "Kullanıcı adınızı yazmalısınız")]
            [Display(Name = "Kullanıcı adı")]
            [MaxLength(15, ErrorMessage = "Çok uzun, en fazla 15 karakter")]
            [MinLength(3, ErrorMessage = "Çok kısa")]
            [RegularExpression(@"[0-9a-z]+", ErrorMessage = "Sadece küçük harflerle latin karakterler ve rakamlar")]
            public string Username { get; set; }
        }

        [HttpPatch("username")]
        public async Task<IActionResult> Username(UsernameModel model)
        {
            ReturnModel rm = new ReturnModel();

            if (!ModelState.IsValid)
            {
                rm.Message = "İstenen bilgileri, geçerli bir şekilde giriniz";
                return Error(rm);
            }
            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (AuthenticatedUser.PasswordCheck(model.UsernameConfirmPassword))
            {
                if (!blockedUsernames.Any(x => model.Username == x))
                {
                    model.Username = model.Username.ToLower();
                    if (AuthenticatedUser.Username != model.Username)
                    {
                        AuthenticatedUser.Username = model.Username;
                        try
                        {
                            AuthenticatedUser.LastChangedOn = DateTime.Now;
                            await Context.SaveChangesAsync();
                            await SignInWithCookie(AuthenticatedUser).ConfigureAwait(false);
                            rm.Data = AuthenticatedUser.Username;
                            rm.Message = "Yeni kullanıcı adınız: " + AuthenticatedUser.Username;
                            return Succes(rm);
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException.Message.Contains("Unique_Key_Username"))
                            {
                                rm.Message = "Bu kullanıcı adını: " + AuthenticatedUser.Username + " kullanamazsınız";
                            }
                            else
                            {
                                rm.Message = "Başaramadık, ne olduğunu bilmiyoruz";
                            }
                            return Error(rm);
                        }
                    }

                    rm.Message = "Aynı değeri girdiniz";
                    return Error(rm);
                }
                else
                {
                    rm.Message = "Bu kullanıcı adını: " + model.Username + " kullanamazsınız";
                    return Error(rm);
                }
            }
            else
            {
                rm.Message = "Kimliğinizi doğrulayamadık: Onay parolası";
                return Error(rm);
            }
        }

        public class PasswordModel
        {
            [Required(ErrorMessage = "Kimliğinizi doğrulamak için şuan ki parolanızı girmelisiniz")]
            [Display(Name = "Onay parolası")]
            [DataType(DataType.Password)]
            public string PasswordConfirmPassword { get; set; }

            [Required(ErrorMessage = "Parola seçmelisiniz")]
            [Display(Name = "Parola")]
            [DataType(DataType.Password)]
            [MinLength(7, ErrorMessage = "En az 7 karakterden oluşan bir şifre oluşturun")]
            [MaxLength(30, ErrorMessage = "Çok uzun, en fazla 30 karakter")]
            public string Password { get; set; }
        }

        [HttpPatch("password")]
        public async Task<IActionResult> Password(PasswordModel model)
        {
            ReturnModel rm = new ReturnModel();

            if (!ModelState.IsValid)
            {
                rm.Message = "İstenen bilgileri, geçerli bir şekilde giriniz";
                return Error(rm);
            }

            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthenticatedUser.PasswordCheck(model.PasswordConfirmPassword))
            {
                model.Password = model.Password.SHA512();

                if (AuthenticatedUser.Password != model.Password)
                {
                    AuthenticatedUser.Password = model.Password;
                    AuthenticatedUser.LastChangedOn = DateTime.Now;
                    await Context.SaveChangesAsync();
                }

                rm.Message = "Parolanız değiştirildi";
                return Succes(rm);
            }
            else
            {
                rm.Message = "Kimliğinizi doğrulayamadık: Onay parolası";
                return Error(rm);
            }
        }

        public class ContactModel
        {
            [Display(Name = "Twitter")]
            [MaxLength(15, ErrorMessage = "Lütfen en fazla 15 karakter giriniz.")]
            [DataType(DataType.Text)]
            public string Twitter { get; set; }

            [Display(Name = "Github")]
            [MaxLength(39, ErrorMessage = "Lütfen en fazla 39 karakter giriniz.")]
            [DataType(DataType.Text)]
            public string Github { get; set; }

            [Display(Name = "E-mail")]
            [MaxLength(50, ErrorMessage = "Lütfen en fazla 50 karakter giriniz.")]
            [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Lütfen geçerli bir e-mail adresi olsun")]
            public string ContactEmail { get; set; }
        }

        [HttpPatch("contact")]
        public async Task<IActionResult> Contact(ContactModel model)
        {
            ReturnModel rm = new ReturnModel();

            if (!ModelState.IsValid)
            {
                rm.Message = "İstenen bilgileri, geçerli bir şekilde giriniz";
                return Error(rm);
            }

            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthenticatedUser.ContactEmail != model.ContactEmail || AuthenticatedUser.Twitter != model.Twitter || AuthenticatedUser.Github != model.Github)
            {
                if (AuthenticatedUser.ContactEmail != model.ContactEmail)
                {
                    AuthenticatedUser.ContactEmail = model.ContactEmail;
                    AuthenticatedUser.LastChangedOn = DateTime.Now;
                }
                if (AuthenticatedUser.Twitter != model.Twitter)
                {
                    AuthenticatedUser.Twitter = model.Twitter;
                    AuthenticatedUser.LastChangedOn = DateTime.Now;
                }
                if (AuthenticatedUser.Github != model.Github)
                {
                    AuthenticatedUser.Github = model.Github;
                    AuthenticatedUser.LastChangedOn = DateTime.Now;
                }

                await Context.SaveChangesAsync();
                rm.Message = "İletişim bilgileriniz kaydedildi";
                return Succes(rm);

            }
            else
            {
                rm.Message = "Aynı içeriklerle değişiklik yapamazsınız";
                return Error(rm);
            }

        }

        public class ProfileModel
        {
            [Required(ErrorMessage = "Adınızı ve soyadınızı yazmalısınız")]
            [Display(Name = "Adınız ve soyadınız")]
            [MaxLength(50, ErrorMessage = "Çok uzun, en fazla 50 karakter")]
            [MinLength(5, ErrorMessage = "Çok kısa")]
            [RegularExpression(@"[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+", ErrorMessage = "Gerçekten adınız ve soyadınız da harf dışında karakter mı var?")]
            public string FullName { get; set; }

            [Display(Name = "Hakkında")]
            [MaxLength(124, ErrorMessage = "Lütfen en fazla 124 karakter giriniz.")]
            [DataType(DataType.MultilineText)]
            public string Biography { get; set; }
        }

        [HttpPatch("profile")]
        public async Task<IActionResult> Profile(ProfileModel model) //editing, adding bio and fullname
        {
            ReturnModel rm = new ReturnModel();

            if (!ModelState.IsValid)
            {
                rm.Message = "İstenen bilgileri, geçerli bir şekilde giriniz";
                return Error(rm);
            }

            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthenticatedUser.Biography != model.Biography || AuthenticatedUser.FullName != model.FullName)
            {
                if (AuthenticatedUser.Biography != model.Biography)
                {
                    AuthenticatedUser.Biography = model.Biography;
                    AuthenticatedUser.LastChangedOn = DateTime.Now;
                }
                if (AuthenticatedUser.Biography != model.FullName)
                {
                    AuthenticatedUser.FullName = model.FullName;
                    AuthenticatedUser.LastChangedOn = DateTime.Now;
                }

                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    rm.Message = "Profil bilgileriniz kaydedildi";
                    return Succes(rm);
                }
            }

            rm.Code = 1001;
            return Error(rm);
        }

        [HttpPatch("photo")]
        public async Task<IActionResult> Photo(IFormFile File)
        {
            ReturnModel rm = new ReturnModel();
            var warning = LocalStorage.WarnAcceptability(File, FileType.Photo);

            if (warning != null)
            {
                rm.Message = warning;
                return Error(rm);
            }

            var uploadedPhotoPathAfterRoot = LocalStorage.UploadProfilePhoto(File, Environment.WebRootPath);
            if (uploadedPhotoPathAfterRoot == null)
            {
                return Error(rm);
            }

            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync().ConfigureAwait(false);
            string oldPhotoPath = AuthenticatedUser.PictureUrl;
            AuthenticatedUser.PictureUrl = uploadedPhotoPathAfterRoot;
            AuthenticatedUser.LastChangedOn = DateTime.Now;

            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                await SignInWithCookie(AuthenticatedUser);

                if (oldPhotoPath != null)
                {
                    LocalStorage.DeleteIfExists(Environment.WebRootPath + oldPhotoPath);
                }

                rm.Data = new { photo = uploadedPhotoPathAfterRoot };
                return Succes(rm);
            }
            else
            {
                LocalStorage.DeleteIfExists(Environment.WebRootPath + uploadedPhotoPathAfterRoot);
                rm.Code = 1001;
                return Error(rm);
            }
        }

        [HttpPatch("photo/remove")]
        public async Task<IActionResult> RemovePhoto()
        {
            ReturnModel rm = new ReturnModel();

            if (User.Identity.GetPhoto() != null)
            {
                AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();

                string OldPhoto = AuthenticatedUser.PictureUrl;

                AuthenticatedUser.PictureUrl = null;
                AuthenticatedUser.LastChangedOn = DateTime.Now;

                await Context.SaveChangesAsync();
                await SignInWithCookie(AuthenticatedUser);

                LocalStorage.DeleteIfExists(Environment.WebRootPath + OldPhoto);
                return Succes(rm);
            }

            return Error(rm);
        }

        public class BirthdateModel
        {
            [Required]
            public int Year { get; set; }
            [Required]
            public int Day { get; set; }
            [Required]
            public int Month { get; set; }
            [Required]
            public bool AreBDMonthDayPublic { get; set; }
            [Required]
            public bool IsBDYearPublic { get; set; }
        }

        [HttpPatch("birthdate")]
        public async Task<IActionResult> Birthdate(BirthdateModel model)
        {
            ReturnModel rm = new ReturnModel();
            if (!ModelState.IsValid)
            {
                rm.Message = "İstenen bilgileri, geçerli bir şekilde giriniz";
                return Error(rm);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();
                AuthenticatedUser.BirthDate = DateTime.Parse(model.Day + "/" + model.Month + "/" + model.Year);
                AuthenticatedUser.AreBDMonthDayPublic = model.AreBDMonthDayPublic;
                AuthenticatedUser.IsBDYearPublic = model.IsBDYearPublic;

                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    return Succes(rm);
                }
                else
                {
                    rm.Code = 1001;
                    return Error(rm);
                }
            }
        }
    }
}
