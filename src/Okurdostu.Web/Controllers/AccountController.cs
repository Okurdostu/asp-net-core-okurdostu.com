using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using Okurdostu.Web.Services;
using System;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController<AccountController>
    {
        private User AuthenticatedUser;
        #region account
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendConfirmationEmail() //it's used on /beta/index page
        {
            //if user doesn't confirm their email, user will see a warning on beta/index page.
            //and this httppost coming there.
            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (!AuthenticatedUser.IsEmailConfirmed)
            {
                var _UserEmailConfirmation = await Context.UserEmailConfirmation.FirstOrDefaultAsync(x => x.UserId == AuthenticatedUser.Id && !x.IsUsed);

                var Email = new OkurdostuEmail((IEmailConfigurationService)HttpContext?.RequestServices.GetService(typeof(IEmailConfigurationService)))
                {
                    SenderMail = "halil@okurdostu.com",
                    SenderName = "Halil İbrahim Kocaöz"
                };

                Guid confirmationGuid;
                if (_UserEmailConfirmation != null)
                {
                    confirmationGuid = _UserEmailConfirmation.GUID;
                }
                else
                {
                    var newUserEmailConfirmation = new UserEmailConfirmation()
                    {
                        UserId = AuthenticatedUser.Id,
                    };
                    await Context.AddAsync(newUserEmailConfirmation);
                    await Context.SaveChangesAsync();
                    confirmationGuid = newUserEmailConfirmation.GUID;
                }

                Email.Send(Email.NewUserMail(AuthenticatedUser.FullName, AuthenticatedUser.Email, confirmationGuid));
                TempData["ProfileMessage"] = AuthenticatedUser.Email + " adresine yeni bir onay maili gönderildi";
            }

            return Redirect("/" + AuthenticatedUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GetConfirmationToEmailChange(ProfileModel Model)
        {
            //authuser must confirm their identity with password if user doesn't this, 
            //user can't show 'CreateEmailChangeRequest' and can't do httppost to 'CreateEmailChangeRequest'.
            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync().ConfigureAwait(false);
            if (AuthenticatedUser.PasswordCheck(Model.ConfirmPassword))
            {
                TempData.Set("IsEmailChangingConfirmedwithPassword", true);
                return Redirect("/account/changeemail");
            }
            else
            {
                TempData["ProfileMessage"] = "Kimliğinizi doğrulayamadık";
                return Redirect("/" + AuthenticatedUser.Username);
            }

        }

        [Route("account/changeemail")]
        public IActionResult CreateEmailChangeRequest()
        {
            //IsEmailChangingConfirmedwithPassword is coming from GetConfirmationToEmailChange IActionResult.
            //user has one chance to input new e-mail adress
            //if this page renewed or user left it, can't see again

            if (TempData.Get<bool>("IsEmailChangingConfirmedwithPassword"))
            {
                TempData.Set("IsEmailChangingConfirmedwithPasswordForPost", true);
                return View();
            }
            else
            {
                TempData.Clear();
                return Unauthorized();
            }

        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/changeemail")]
        public async Task<IActionResult> CreateEmailChangeRequest(ProfileModel Model)
        {
            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (TempData.Get<bool>("IsEmailChangingConfirmedwithPasswordForPost"))
            {
                TempData.Clear();

                Model.Email = Model.Email.ToLower();

                bool IsThereAnyUserWithThatEmailAdress = await Context.User.AnyAsync(x => x.Email == Model.Email);

                if (IsThereAnyUserWithThatEmailAdress is false)
                {
                    if (AuthenticatedUser.Email != Model.Email)
                    {

                        var Email = new OkurdostuEmail((IEmailConfigurationService)HttpContext?.RequestServices.GetService(typeof(IEmailConfigurationService)))
                        {
                            SenderMail = "noreply@okurdostu.com",
                            SenderName = "Okurdostu"
                        };

                        var RequestWithSameEmailandUser = await Context.UserEmailConfirmation.FirstOrDefaultAsync(x => x.NewEmail == Model.Email && x.UserId == AuthenticatedUser.Id && !x.IsUsed);

                        if (RequestWithSameEmailandUser == null)
                        {

                            var UserEmailConfirmation = new UserEmailConfirmation() //UserEmailConfirmation'u oluştur
                            {
                                UserId = AuthenticatedUser.Id,
                                NewEmail = Model.Email,
                            };

                            await Context.AddAsync(UserEmailConfirmation);
                            var result = await Context.SaveChangesAsync();
                            if (result > 0)
                            {
                                Email.Send(Email.EmailAddressChangeMail(AuthenticatedUser.FullName, UserEmailConfirmation.NewEmail, UserEmailConfirmation.GUID));
                                TempData["ProfileMessage"] = "Yeni e-mail adresinize (" + UserEmailConfirmation.NewEmail + ") onaylamanız için bir e-mail gönderildi" +
                                    "<br>" +
                                    "Onaylayana kadar şuan ki e-mail adresiniz geçerli kalacaktır.";
                            }
                            else
                            {
                                TempData["ProfileMessage"] = "Bir değişiklik yapılamadı";
                            }

                        }
                        else
                        {
                            Email.Send(Email.EmailAddressChangeMail(AuthenticatedUser.FullName, RequestWithSameEmailandUser.NewEmail, RequestWithSameEmailandUser.GUID));
                            TempData["ProfileMessage"] = "Yeni e-mail adresinize (" + RequestWithSameEmailandUser.NewEmail + ") onaylamanız için bir e-mail gönderildi";
                        }
                    }
                    else
                    {
                        TempData["ProfileMessage"] = "Şuan ki e-mail adresiniz ile aynı değeri giriyorsunuz";
                    }

                }
                else
                {
                    TempData["ProfileMessage"] = "Bu email adresini kullanamazsınız";
                }
            }

            return Redirect("/" + AuthenticatedUser.Username);
        }

        [Route("confirmemail/{guid}")]
        public async Task ConfirmEmail(Guid guid)
        {
            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();
            var ConfirmationRequest = await Context.UserEmailConfirmation.FirstOrDefaultAsync(x => !x.IsUsed && x.UserId == AuthenticatedUser.Id && x.GUID == guid);

            if (ConfirmationRequest != null)
            {
                string OldEmail = AuthenticatedUser.Email;
                bool OldEmailConfirmStatus = AuthenticatedUser.IsEmailConfirmed;

                if (ConfirmationRequest.NewEmail != null)
                {
                    //yeni bir user onaylaması değilde e-mail değiştirme isteği ile geldiyse.
                    AuthenticatedUser.Email = ConfirmationRequest.NewEmail;
                    TempData["ProfileMessage"] = "Yeni e-mail adresiniz hesabınıza eşleştirildi";
                }
                try
                {
                    AuthenticatedUser.IsEmailConfirmed = true;
                    ConfirmationRequest.IsUsed = true;
                    ConfirmationRequest.UsedOn = DateTime.Now;
                    if (TempData["ProfileMessage"] != null)
                    {
                        TempData["ProfileMessage"] += "<br/>E-mail adresiniz onaylandı, teşekkürler";
                    }
                    else
                    {
                        TempData["ProfileMessage"] = "E-mail adresiniz onaylandı, teşekkürler";
                    }
                    AuthenticatedUser.LastChangedOn = DateTime.Now;
                    await Context.SaveChangesAsync();
                    await SignInWithCookie(AuthenticatedUser);
                }
                catch (Exception e)
                {
                    if (e.InnerException != null && e.InnerException.Message.Contains("Unique_Key_Email"))
                    {
                        TempData["ProfileMessage"] = "Değiştirme talebinde bulunduğunuz e-mail adresini kullanamazsınız.<br>E-mail değiştirme isteğiniz geçersiz kılındı<br>Yeni bir e-mail değiştirme isteğinde bulunun";
                        AuthenticatedUser.Email = OldEmail;
                        AuthenticatedUser.IsEmailConfirmed = OldEmailConfirmStatus;
                        ConfirmationRequest.IsUsed = true;
                        ConfirmationRequest.UsedOn = DateTime.Now;
                        await Context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                TempData["ProfileMessage"] = "Sanırım bir hata yapıyorsunuz, size gönderdiğimiz bağlantıyı kullanın";
            }

            Response.Redirect("/" + AuthenticatedUser.Username);
        }
        #endregion
    }
}