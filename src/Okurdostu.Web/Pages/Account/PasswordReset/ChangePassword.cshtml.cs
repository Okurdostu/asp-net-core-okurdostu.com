using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Okurdostu.Web.Pages.Account.PasswordReset
{
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required(ErrorMessage = "Parola seçmelisiniz")]
            [Display(Name = "Parola")]
            [DataType(DataType.Password)]
            [MinLength(7, ErrorMessage = "En az 7 karakterden oluşan bir parola oluşturun")]
            [MaxLength(30, ErrorMessage = "Çok uzun, en fazla 30 karakter")]
            public string Password { get; set; }
        }

        public OkurdostuContext Context =>  (OkurdostuContext)HttpContext?.RequestServices.GetService(typeof(OkurdostuContext));
        public async Task<IActionResult> OnGet(Guid guid)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            var _UserPaswordReset = await Context.UserPasswordReset.FirstOrDefaultAsync(x => x.GUID == guid && !x.IsUsed);

            if (_UserPaswordReset != null)
            {
                var elapsedTime = DateTime.Now - _UserPaswordReset.CreatedOn;

                if (elapsedTime.TotalHours < 12)
                {
                    TempData["Guid"] = _UserPaswordReset.GUID;
                    return Page();
                }
                else
                {
                    Context.Remove(_UserPaswordReset);
                    await Context.SaveChangesAsync();
                }
            }
            return NotFound("Böyle bir şey yok");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            if (ModelState.IsValid)
            {
                Guid guid = Guid.Empty;
                if (TempData["Guid"]?.ToString() != null)
                {
                    guid = Guid.Parse(TempData["Guid"]?.ToString());
                    TempData.Clear();
                }

                var _UserPaswordReset = await Context.UserPasswordReset.FirstOrDefaultAsync(x => x.GUID == guid && !x.IsUsed);
                if (_UserPaswordReset != null)
                {
                    var elapsedTime = DateTime.Now - _UserPaswordReset.CreatedOn;

                    if (elapsedTime.TotalHours < 12)
                    {
                        var User = await Context.User.FirstOrDefaultAsync(x => x.Id == _UserPaswordReset.UserId);
                        if (User != null && User.IsActive)
                        {
                            User.Password = Input.Password.SHA512();
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
                                TempData["ChangePasswordMessage"] = "Şifrenizi değiştiremedik, lütfen tekrar deneyin<br />" +
                                    "Şuan ki şifreniz ile aynı şifreyi giriyor olabilirsiniz";
                                return Redirect("~/account/passwordreset/changepassword/" + _UserPaswordReset.GUID);
                            }
                        }
                    }
                    else
                    {
                        Context.Remove(_UserPaswordReset);
                        await Context.SaveChangesAsync();
                    }
                }
            }
            return NotFound("Böyle bir şey yok");
        }
    }
}
