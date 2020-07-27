using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    public class AccountController : SecureApiController
    {
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

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost("username")]
        public async Task<IActionResult> Username(UsernameModel model)
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (!ModelState.IsValid)
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "İstenen bilgileri, geçerli bir şekilde giriniz";
                return Error(jsonReturnModel);
            }

            if (await ConfirmIdentityWithPassword(model.UsernameConfirmPassword))
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
                            await SignInWithCookie(AuthenticatedUser);
                            jsonReturnModel.Data = AuthenticatedUser.Username;
                            jsonReturnModel.Message = "Yeni kullanıcı adınız: " + AuthenticatedUser.Username;
                            return Succes(jsonReturnModel);
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException.Message.Contains("Unique_Key_Username"))
                            {
                                jsonReturnModel.Message = "Bu kullanıcı adını: " + AuthenticatedUser.Username + " kullanamazsınız";
                            }
                            else
                            {
                                jsonReturnModel.Message = "Başaramadık, ne olduğunu bilmiyoruz";
                            }
                        }
                    }
                    jsonReturnModel.Code = 200;
                    jsonReturnModel.Message = "Aynı değeri girdiniz";
                    return Error(jsonReturnModel);
                }
                else
                {
                    jsonReturnModel.Code = 200;
                    jsonReturnModel.Message = "Bu kullanıcı adını: " + model.Username + " kullanamazsınız";
                    return Error(jsonReturnModel);
                }
            }
            else
            {
                jsonReturnModel.Message = "Kimliğinizi doğrulayamadık: Onay parolası";
                jsonReturnModel.Code = 200;
                return Error(jsonReturnModel);
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

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost("password")]
        public async Task<IActionResult> Password(PasswordModel model)
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (!ModelState.IsValid)
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "İstenen bilgileri, geçerli bir şekilde giriniz";
                return Error(jsonReturnModel);
            }

            if (await ConfirmIdentityWithPassword(model.PasswordConfirmPassword))
            {
                model.Password = model.Password.SHA512();

                if (AuthenticatedUser.Password != model.Password)
                {
                    AuthenticatedUser.Password = model.Password;
                    AuthenticatedUser.LastChangedOn = DateTime.Now;
                    var result = await Context.SaveChangesAsync();

                    if (result <= 0)
                    {
                        jsonReturnModel.Code = 200;
                        jsonReturnModel.Message = "Başaramadık, ne olduğunu bilmiyoruz";
                        return Error(jsonReturnModel);
                    }
                }

                jsonReturnModel.Message = "Parolanız değiştirildi";
                return Succes(jsonReturnModel);
            }
            else
            {
                jsonReturnModel.Message = "Kimliğinizi doğrulayamadık: Onay parolası";
                jsonReturnModel.Code = 200;
                return Error(jsonReturnModel);
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

        [HttpPost("contact")]
        public async Task<IActionResult> Contact(ContactModel model)
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (!ModelState.IsValid)
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "İstenen bilgileri, geçerli bir şekilde giriniz";
                return Error(jsonReturnModel);
            }

            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();

            AuthenticatedUser.ContactEmail = model.ContactEmail;
            AuthenticatedUser.Twitter = model.Twitter;
            AuthenticatedUser.Github = model.Github;

            AuthenticatedUser.LastChangedOn = DateTime.Now;
            
            var result = await Context.SaveChangesAsync();
            if (result > 0)
            {
                jsonReturnModel.Message = "İletişim bilgileriniz kaydedildi";
                return Succes(jsonReturnModel);
            }
            else
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "Hiç bir değişiklik yapılmadı";
                return Error(jsonReturnModel);
            }
        }
    }
}
