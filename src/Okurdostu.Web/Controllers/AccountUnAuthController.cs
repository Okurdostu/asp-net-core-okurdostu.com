using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Extensions;
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
                var elapsedTime = DateTime.Now - preCreatedPaswordReset?.CreatedOn;

                if (preCreatedPaswordReset != null && elapsedTime.Value.Hours < 11.5)
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
        public async Task<IActionResult> ResetPassword(Guid guid)
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            var _UserPaswordReset = await Context.UserPasswordReset.FirstOrDefaultAsync(x => x.GUID == guid && !x.IsUsed);

            if (_UserPaswordReset != null)
            {
                var elapsedTime = DateTime.Now - _UserPaswordReset.CreatedOn;
                if (elapsedTime.Value.Hours < 12)
                {
                    TempData["_UserPasswordResetGuid"] = _UserPaswordReset.GUID;
                    TempData["PasswordResetUser"] = await Context.User.FirstOrDefaultAsync(x => x.Id == _UserPaswordReset.UserId);
                    return View();
                }
                else
                {
                    Context.Remove(_UserPaswordReset);
                    await Context.SaveChangesAsync();
                }
            }
            return NotFound("Böyle bir şey yok");

        }

        //changing the password
        [Route("~/account/changepassword")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileModel Model) //it's changing password
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            Guid guid = Guid.Empty;
            if (TempData["_UserPasswordResetGuid"]?.ToString() != null)
            {
                guid = Guid.Parse(TempData["_UserPasswordResetGuid"]?.ToString());
                TempData.Clear();
            }

            var _UserPaswordReset = await Context.UserPasswordReset.FirstOrDefaultAsync(x => x.GUID == guid && !x.IsUsed);

            if (_UserPaswordReset != null)
            {
                var elapsedTime = DateTime.Now - _UserPaswordReset.CreatedOn;
                if (elapsedTime.Value.Hours < 12)
                {
                    var User = await Context.User.FirstOrDefaultAsync(x => x.Id == _UserPaswordReset.UserId);
                    User.Password = Model.Password.SHA512();
                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {
                        _UserPaswordReset.UsedOn = DateTime.Now;
                        _UserPaswordReset.IsUsed = true;
                        await Context.SaveChangesAsync();
                        TempData["LoginMessage"] = "Giriş için yeni şifrenizi kullanabilirsiniz";
                        return Redirect("/girisyap");
                    }
                    else
                    {
                        TempData["ResetPasswordMessage"] = "Şifrenizi değiştiremedik, lütfen tekrar deneyin<br />" +
                            "Şuan ki şifreniz ile aynı şifreyi giriyor olabilirsiniz";
                        return Redirect("~/account/resetpassword/" + guid.ToString());
                    }
                }
                else
                {
                    Context.Remove(_UserPaswordReset);
                    await Context.SaveChangesAsync();
                }
            }
            return NotFound("Böyle bir şey yok");

        }
        #endregion
    }
}