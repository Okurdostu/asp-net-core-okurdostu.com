using Microsoft.AspNetCore.Mvc;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    public class PasswordController : ApiController
    {


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
        [HttpPost("post")]
        public async Task<IActionResult> Post(PasswordModel model)
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
    }
}
