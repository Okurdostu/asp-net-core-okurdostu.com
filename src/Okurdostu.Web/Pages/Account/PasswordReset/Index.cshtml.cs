using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;

namespace Okurdostu.Web.Pages.Account.PasswordReset
{
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required(ErrorMessage = "Bu bilgiyi doldurmalısınız")]
            [Display(Name = "Kullanıcı adı, e-mail veya telefon numarası")]
            [MaxLength(25, ErrorMessage = "Çok uzun, en fazla 25 karakter")]
            public string IdentificationValue { get; set; }
        }

        private OkurdostuContext _con;
        public OkurdostuContext Context => _con ?? (OkurdostuContext)HttpContext?.RequestServices.GetService(typeof(OkurdostuContext));

        public void OnGet()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Response.Redirect("/");
            }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            var User = await Context.User.FirstOrDefaultAsync(x => x.Username == Input.IdentificationValue || x.Telephone == Input.IdentificationValue || x.Email == Input.IdentificationValue && x.IsActive);
            
            if (User != null)
            {
                TempData.Set("resetPasswordUserId", User.Id.ToString());

                if (User.Username == Input.IdentificationValue)
                {
                    TempData.Set("resetPasswordEmail", User.Email.StarsToEmail());
                    TempData.Set("IdentificationType", "Username");
                }
                else if (User.Email == Input.IdentificationValue)
                {
                    TempData.Set("resetPasswordEmail", User.Email);
                    TempData.Set("IdentificationType", "Email");
                }
                else
                {
                    TempData.Set("resetPasswordEmail", User.Email.StarsToEmail());
                    TempData.Set("IdentificationType", "Telephone");
                }

                return Redirect("~/account/passwordreset/sendmail");
            }
            else
            {
                TempData["PasswordResetMessage"] = "Böyle bir kullanıcı yok";
            }

            return Redirect("~/account/passwordreset");
        }
    }
}