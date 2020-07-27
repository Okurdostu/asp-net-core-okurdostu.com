using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Okurdostu.Web.Base;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    public class UsernameController : ApiController
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

        [Authorize]
        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost("post")]
        public async Task<IActionResult> Post(UsernameModel model)
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
                                jsonReturnModel.Code = 200;
                                jsonReturnModel.Message = "Bu kullanıcı adını: " + AuthenticatedUser.Username + " kullanamazsınız";
                                return Error(jsonReturnModel);
                            }
                            else
                            {
                                jsonReturnModel.Code = 200;
                                jsonReturnModel.Message = "Başaramadık, ne olduğunu bilmiyoruz";
                                return Error(jsonReturnModel);
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
    }
}