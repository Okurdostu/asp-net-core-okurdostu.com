using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Models;

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


        //sending post to PasswordResetShowUser
        [Route("~/account/beginpasswordreset")]
        [Route("~/account/beginpasswordreset/{DefineValue}")]
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
        public async Task<IActionResult> PasswordResetShowUser(PasswordResetModel Model)
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
                    //user bilgileri (kullanıcı adı, fullname, profil fotoğrafı), sadece kullanıcı adı ile bulunduysa gösterilecek..
                    //e-mail veya telefon numarası ile bulunduysa user temel bilgilerini görüntülemeyeceğiz sadece: parola sıfırlaması için key gönderilecek user e-maili gösterilecek
                    //user e-mailinin bir kısmı gösterilecek..

                    return View(User);
                }
                else
                {
                    TempData[""] = "Kullanıcı bulunamadı, verdiğiniz bilgilerle eşleşen bir kullanıcı yok";
                }
            }
            else
            {
                TempData[""] = "Doldurmanız gereken bilgileri istenen şekilde doldurun.";
            }

            return Redirect("~/account/beginpasswordreset/" + Model.IdentificationValue);

        }


        //sending key to usermail for reset the password
        [Route("~/account/sendpasswordresetkey")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SendPasswordResetKey(User user)
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            if (User != null)
            {
                //veritabanı key oluşturma
                var Email = new OkurdostuEmail(null)
                {
                    SenderMail = "noreply@okurdostu.com",
                    SenderName = "Okurdostu"
                };
                //keyli linki yollama
                //..
                //..
                TempData[""] = "Parolanızı sıfırlamanız için bir e-mail gönderildi.";
            }
            else
            {
                TempData[""] = "Kullanıcı bulunamadı";
            }

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