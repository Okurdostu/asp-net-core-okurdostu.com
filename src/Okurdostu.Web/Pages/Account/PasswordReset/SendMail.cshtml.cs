using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Data.Model;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Services;

namespace Okurdostu.Web.Pages.Account.PasswordReset
{
    [ValidateAntiForgeryToken]
    public class SendMailModel : PageModel
    {
        public OkurdostuContext Context => (OkurdostuContext)HttpContext?.RequestServices.GetService(typeof(OkurdostuContext));
        public User _User { get; set; }

        public IActionResult OnGet()
        {
            var UserId = TempData.Get<string>("resetPasswordUserId");
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            else if (UserId == null || TempData["IdentificationType"] == null)
            {
                TempData.Clear();
                return Redirect("/account/passwordreset");
            }

            _User = Context.User.FirstOrDefault(x => x.Id == long.Parse(UserId) && x.IsActive);

            if (_User != null)
            {
                TempData.Set("resetPasswordUserId", _User.Id.ToString());
                return Page();
            }
            else
            {
                TempData["PasswordResetMessage"] = "Kullanıcıya ulaşılamadı";
                return Redirect("/account/passwordreset");
            }

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var UserId = TempData.Get<string>("resetPasswordUserId");
            TempData.Remove("resetPasswordUserId");
            _User = Context.User.FirstOrDefault(x => x.Id == long.Parse(UserId) && x.IsActive);

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            else if (_User == null)
            {
                TempData["PasswordResetMessage"] = "Kullanıcıya ulaşılamadı";
                return Redirect("/account/passwordreset");
            }

            var Email = new OkurdostuEmail((IEmailConfiguration)HttpContext?.RequestServices.GetService(typeof(IEmailConfiguration)))
            {
                SenderMail = "noreply@okurdostu.com",
                SenderName = "Okurdostu"
            };

            var preCreatedPaswordReset = Context.UserPasswordReset.Where(x => x.UserId == _User.Id && !x.IsUsed).Include(x => x.User).ToList().LastOrDefault();
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
            return Redirect("/account/passwordreset/successent");
        }
    }
}