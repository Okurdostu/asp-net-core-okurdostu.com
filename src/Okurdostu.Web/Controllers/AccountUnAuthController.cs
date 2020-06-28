using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Models;
using Okurdostu.Web.Services;

namespace Okurdostu.Web.Controllers
{
    //unauthorized tasks for user accounts
    public class AccountUnAuthController : BaseController<AccountUnAuthController>
    {

        #region passwordreset
        public class PasswordResetModel
        {
            [Required(ErrorMessage = "Bu bilgiyi doldurmalısınız")]
            [Display(Name = "Kullanıcı adı, e-mail veya telefon numarası")]
            [MaxLength(25, ErrorMessage = "Çok uzun, en fazla 25 karakter")]
            public string IdentificationValue { get; set; }
        }


        //sending post to PasswordResetPost
        [Route("~/account/beginpasswordreset")]
        [Route("~/account/beginpasswordreset/{IdentificationValue}")]
        public IActionResult PasswordReset(string IdentificationValue)
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            return View();

        }


        //finding user by the params that coming, showing finded user. sending post to SendPasswordResetKey
        [Route("~/account/passwordreset/")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordResetPost(PasswordResetModel Model)
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            if (ModelState.IsValid)
            {
                var User = await Context.User.FirstOrDefaultAsync(x => x.Username == Model.IdentificationValue || x.Telephone == Model.IdentificationValue || x.Email == Model.IdentificationValue);
                if (User != null)
                {
                    TempData["userid"] = User.Id.ToString();

                    #region viewkuralları
                    if (User.Username == Model.IdentificationValue)
                    {
                        //kullanıcı profilinin temel bilgilerini göster.(fullname, profile photo, username)
                        //e-mailin bir kısmını göster
                        TempData["IdentificationType"] = "Username";
                    }
                    else if (User.Email == Model.IdentificationValue)
                    {
                        //kullanıcı profili hakkında hiç bir bilgi gösterme
                        //e-mailin tamamını göster
                        TempData["IdentificationType"] = "Email";
                    }
                    else
                    {
                        //kullanıcı profili hakkında hiç bir bilgi gösterme
                        //e-mailin bir kısmını göster
                        TempData["IdentificationType"] = "Telephone";
                    }
                    #endregion

                    return View(User);
                }
                else
                {
                    TempData["PasswordResetMessage"] = "Kullanıcı yok, bu bilgi ile eşleşen bir kullanıcı yok";
                }
            }
            else
            {
                TempData["PasswordResetMessage"] = "Doldurmanız gereken bilgileri istenen şekilde doldurun";
            }

            return Redirect("~/account/beginpasswordreset/" + Model.IdentificationValue);

        }


        //sending key to usermail for reset the password
        [Route("~/account/sendpasswordresetkey")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPasswordResetKey()
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            var UserId = TempData["userid"]?.ToString();
            var _User = Context.User.FirstOrDefault(x => x.Id == int.Parse(UserId));
            TempData.Clear();
            if (_User != null)
            {
                var Email = new OkurdostuEmail((IEmailConfiguration)HttpContext?.RequestServices.GetService(typeof(IEmailConfiguration)))
                {
                    SenderMail = "noreply@okurdostu.com",
                    SenderName = "Okurdostu"
                };

                var preCreatedPaswordReset = await Context.UserPasswordReset.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == _User.Id && !x.IsUsed);
                var x = DateTime.Now - preCreatedPaswordReset?.CreatedOn;

                //kullanıcı için önceden oluşturulmuş bir password reset guid varsa ve o keyin oluşturulmasından şuan ki zamana kadar 
                //11.5 saatten az bir süre geçtiyse onu seçip onu key olarak mail yolluyoruz.
                if (preCreatedPaswordReset != null && x.Value.Hours < 11.5)
                {
                    Email.Send(Email.PasswordResetMail(preCreatedPaswordReset.User.FullName, preCreatedPaswordReset.User.Email, preCreatedPaswordReset.GUID));
                }
                else
                {
                    var UserPaswordReset = new UserPasswordReset()
                    {
                        UserId = _User.Id
                    };
                    await Context.AddAsync(UserPaswordReset);
                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {
                        Email.Send(Email.PasswordResetMail(_User.FullName, _User.Email, UserPaswordReset.GUID));
                    }
                }
                // return e-mailin gönderildiğine dair bir sayfa gösterip, orada uyarılarda bulun.
            }
            TempData["PasswordResetMessage"] = "Kullanıcıya ulaşılamadı";
            return Redirect("/account/beginpasswordreset/");

        }


        //catching the key and sending post ChangePassword
        [Route("~/account/resetpassword/{guid}")]
        public IActionResult ResetPassword(Guid guid)
        {
            //keyin oluşturulması üzerinden 12 saat geçmişse key yakalansa bile yok edilecek, şifre değiştirmeye izin verilmeyecek.
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            return View();

        }


        //changing the password
        [HttpPost]
        public IActionResult ChangePassword(ProfileModel Model) //it's changing password
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            return View();

        }
        #endregion
    }
}