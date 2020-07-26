using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using Okurdostu.Web.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController<AccountController>
    {
        private User AuthUser;

#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IHostingEnvironment Environment;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
        public AccountController(IHostingEnvironment env) => Environment = env;
#pragma warning restore CS0618 // Type or member is obsolete

        #region nonaction
        [NonAction]
        public async Task<bool> ConfirmIdentityWithPassword(string ConfirmPassword)
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            ConfirmPassword = ConfirmPassword.SHA512();
            return ConfirmPassword == AuthUser.Password;
        }

        [NonAction]
        public bool DeleteFileFromServer(string filePathAfterRootPath)
        {
            if (System.IO.File.Exists(Environment.WebRootPath + filePathAfterRootPath))
            {
                System.IO.File.Delete(Environment.WebRootPath + filePathAfterRootPath);
                Logger.LogInformation("User({Id}) | A file deleted on server: {file}", AuthUser?.Id, filePathAfterRootPath);
                return true;
            }
            else
            {
                Logger.LogWarning("File deleting failed, there isn't a file: " + filePathAfterRootPath);
                return false;
            }
        }
        #endregion


        #region account
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendConfirmationEmail() //it's used on /beta/index page
        {
            //if user doesn't confirm their email, user will see a warning on beta/index page.
            //and this httppost coming there.
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (!AuthUser.IsEmailConfirmed)
            {
                var _UserEmailConfirmation = await Context.UserEmailConfirmation.FirstOrDefaultAsync(x => x.UserId == AuthUser.Id && !x.IsUsed);

                var Email = new OkurdostuEmail((IEmailConfiguration)HttpContext?.RequestServices.GetService(typeof(IEmailConfiguration)))
                {
                    SenderMail = "halil@okurdostu.com",
                    SenderName = "Halil İbrahim Kocaöz"
                };

                Guid confirmationGuid = Guid.Empty;
                if (_UserEmailConfirmation != null)
                {
                    confirmationGuid = _UserEmailConfirmation.Guid;
                }
                else
                {
                    var newUserEmailConfirmation = new UserEmailConfirmation()
                    {
                        UserId = AuthUser.Id,
                    };
                    await Context.AddAsync(newUserEmailConfirmation);
                    await Context.SaveChangesAsync();
                    confirmationGuid = newUserEmailConfirmation.Guid;
                }

                Email.Send(Email.NewUserMail(AuthUser.FullName, AuthUser.Email, confirmationGuid));
                TempData["ProfileMessage"] = AuthUser.Email + " adresine yeni bir onay maili gönderildi";
            }

            return Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GetConfirmationToEmailChange(ProfileModel Model)
        {
            //authuser must confirm their identity with password if user doesn't this, 
            //user can't show 'CreateEmailChangeRequest' and can't do httppost to 'CreateEmailChangeRequest'.
            if (await ConfirmIdentityWithPassword(Model.ConfirmPassword))
            {
                TempData.Set("IsEmailChangingConfirmedwithPassword", true);
                return Redirect("/account/changeemail");
            }
            else
            {
                TempData["ProfileMessage"] = "Kimliğinizi doğrulayamadık";
                return Redirect("/" + AuthUser.Username);
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
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (TempData.Get<bool>("IsEmailChangingConfirmedwithPasswordForPost"))
            {
                TempData.Clear();

                Model.Email = Model.Email.ToLower();

                bool IsThereAnyUserWithThatEmailAdress = await Context.User.AnyAsync(x => x.Email == Model.Email);

                if (!IsThereAnyUserWithThatEmailAdress)
                {
                    if (AuthUser.Email != Model.Email)
                    {

                        var Email = new OkurdostuEmail((IEmailConfiguration)HttpContext?.RequestServices.GetService(typeof(IEmailConfiguration)))
                        {
                            SenderMail = "noreply@okurdostu.com",
                            SenderName = "Okurdostu"
                        };

                        var RequestWithSameEmailandUser = await Context.UserEmailConfirmation.FirstOrDefaultAsync(x => x.NewEmail == Model.Email && x.UserId == AuthUser.Id && !x.IsUsed);

                        if (RequestWithSameEmailandUser == null)
                        {

                            var UserEmailConfirmation = new UserEmailConfirmation() //UserEmailConfirmation'u oluştur
                            {
                                UserId = AuthUser.Id,
                                NewEmail = Model.Email, //değiştirilmesini istediği email newemail olarak kolona al

                                //bu veri kolonu emailconfirmation/guid ile geldiği zaman
                                //newemail kolonu yakalanıp veri varsa kullanıcıya direkt olarak o e-maili atayıp
                                //onay mailini de yeni e-maile yolladığımız için e-mail adresini onaylayacak.
                                //yani aslında buralarda e-mailini değiştirmiyoruz confirmemail aşamasında e-mail adresi değişiyor.
                            };

                            await Context.AddAsync(UserEmailConfirmation);
                            var result = await Context.SaveChangesAsync();
                            if (result > 0)
                            {
                                Email.Send(Email.EmailAddressChangeMail(AuthUser.FullName, UserEmailConfirmation.NewEmail, UserEmailConfirmation.Guid));
                                TempData["ProfileMessage"] = "Yeni e-mail adresinize (" + UserEmailConfirmation.NewEmail + ") onaylamanız için bir e-mail gönderildi";
                            }
                            else
                            {
                                TempData["ProfileMessage"] = "Bir değişiklik yapılamadı";
                            }

                        }
                        else
                        {
                            Email.Send(Email.EmailAddressChangeMail(AuthUser.FullName, RequestWithSameEmailandUser.NewEmail, RequestWithSameEmailandUser.Guid));
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

            return Redirect("/" + AuthUser.Username);
        }

        [Route("confirmemail/{guid}")]
        public async Task ConfirmEmail(Guid guid)
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            var ConfirmationRequest = await Context.UserEmailConfirmation.FirstOrDefaultAsync(x => !x.IsUsed && x.UserId == AuthUser.Id && x.Guid == guid);

            if (ConfirmationRequest != null)
            {
                string OldEmail = AuthUser.Email;
                bool OldEmailConfirmStatus = AuthUser.IsEmailConfirmed;

                if (ConfirmationRequest.NewEmail != null)
                {
                    //yeni bir user onaylaması değilde e-mail değiştirme isteği ile geldiyse.
                    AuthUser.Email = ConfirmationRequest.NewEmail;
                    TempData["ProfileMessage"] = "Yeni e-mail adresiniz hesabınıza eşleştirildi";
                }
                try
                {
                    AuthUser.IsEmailConfirmed = true;
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
                    AuthUser.LastChangedOn = DateTime.Now;
                    await Context.SaveChangesAsync();
                    await SignInWithCookie(AuthUser);
                }
                catch (Exception e)
                {
                    if (e.InnerException != null && e.InnerException.Message.Contains("Unique_Key_Email"))
                    {
                        TempData["ProfileMessage"] = "Değiştirme talebinde bulunduğunuz e-mail adresini kullanamazsınız.<br>E-mail değiştirme isteğiniz geçersiz kılındı<br>Yeni bir e-mail değiştirme isteğinde bulunun";
                        AuthUser.Email = OldEmail;
                        AuthUser.IsEmailConfirmed = OldEmailConfirmStatus;
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

            Response.Redirect("/" + AuthUser.Username);
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/password")]
        public async Task EditPassword(ProfileModel Model)
        {
            if (await ConfirmIdentityWithPassword(Model.ConfirmPassword))
            {

                Model.Password = Model.Password.SHA512();
                if (AuthUser.Password != Model.Password)
                {
                    AuthUser.Password = Model.Password;
                    AuthUser.LastChangedOn = DateTime.Now;
                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {
                        TempData["ProfileMessage"] = "Artık giriş yaparken yeni parolanızı kullanabilirsiniz";
                        Logger.LogInformation("User({Id}) changed their password: ", AuthUser.Id);
                    }
                    else
                    {
                        TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                        Logger.LogError("Changes aren't save, User({Id}) take error when trying change their password: ", AuthUser.Id);
                    }
                }

            }
            else
            {
                TempData["ProfileMessage"] = "Kimliğinizi doğrulayamadık";
            }

            Response.Redirect("/" + AuthUser.Username);
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/username")]
        public async Task EditUsername(ProfileModel Model)
        {
            if (await ConfirmIdentityWithPassword(Model.ConfirmPassword))
            {
                if (!blockedUsernames.Any(x => Model.Username == x))
                {
                    Model.Username = Model.Username.ToLower();
                    if (AuthUser.Username != Model.Username)
                    {
                        string NowUsername = AuthUser.Username;
                        AuthUser.Username = Model.Username;
                        try
                        {
                            AuthUser.LastChangedOn = DateTime.Now;
                            await Context.SaveChangesAsync();
                            await SignInWithCookie(AuthUser);
                            TempData["ProfileMessage"] = "Yeni kullanıcı adınız: " + AuthUser.Username;
                            Logger.LogInformation("User({Id}) changed their username, old: {old} new: {new}", AuthUser.Id, NowUsername, AuthUser.Username);
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException.Message.Contains("Unique_Key_Username"))
                            {
                                TempData["ProfileMessage"] = "Bu kullanıcı adını: " + AuthUser.Username + " kullanamazsınız";
                            }
                            else
                            {
                                Logger.LogError("Changing username failed, UserId: {Id} Ex message: {ExMessage}", AuthUser.Id, e.Message);
                                TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                            }
                            AuthUser.Username = NowUsername;
                        }
                    }
                }
                else
                {
                    TempData["ProfileMessage"] = "Bu kullanıcı adını: " + Model.Username + " kullanamazsınız";
                }
            }
            else
            {
                TempData["ProfileMessage"] = "Kimliğinizi doğrulayamadık";
            }

            Response.Redirect("/" + AuthUser.Username);
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/contact")]
        public async Task Contact(ProfileModel Model) //editing, adding contacts
        {

            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthUser.ContactEmail != Model.ContactEmail || AuthUser.Twitter != Model.Twitter || AuthUser.Github != Model.Github)
            {
                AuthUser.ContactEmail = Model.ContactEmail;
                AuthUser.Twitter = Model.Twitter;
                AuthUser.Github = Model.Github;
                AuthUser.LastChangedOn = DateTime.Now;
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    Logger.LogInformation("User({Id}) added or changed contact informations", AuthUser.Id);
                }
                else
                {
                    TempData["ProfileMessage"] = "Başaramadık, ne olduğunu bilmiyoruz";
                    Logger.LogError("Changes aren't save, User({Id}) take error when trying add or change contact informations", AuthUser.Id);
                }
            }

            Response.Redirect("/" + AuthUser.Username);

        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/basic")]
        public async Task ProfileBasic(ProfileModel Model) //editing, adding bio and fullname
        {

            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthUser.Biography != Model.Biography || AuthUser.FullName != Model.FullName)
            {
                AuthUser.Biography = Model.Biography;
                AuthUser.FullName = Model.FullName;
                AuthUser.LastChangedOn = DateTime.Now;
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    Logger.LogInformation("User({Id}) changed bio or fullname", AuthUser.Id);
                }
                else
                {
                    TempData["ProfileMessage"] = "Başaramadık, ne olduğunu bilmiyoruz";
                    Logger.LogError("Changes aren't save,User({Id}) take error when trying change bio or fullname", AuthUser.Id);
                }
            }

            Response.Redirect("/" + AuthUser.Username);

        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/photo")]
        public async Task AddPhoto()
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            var File = Request.Form.Files.First();

            if (File != null && File.Length <= 1048576 && File.Length > 0)
            {
                if (File.ContentType == "image/png" || File.ContentType == "image/jpg" || File.ContentType == "image/jpeg")
                {
                    string Name = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName);
                    string FilePathWithName = Environment.WebRootPath + "/image/profil-fotograf/" + Name;

                    using var ImageSharp = Image.Load(File.OpenReadStream());
                    if (ImageSharp.Width > 200)
                    {
                        ImageSharp.Mutate(x => x.Resize(200, 200));
                    }
                    ImageSharp.Save(FilePathWithName);
                    Logger.LogInformation("User({Id}) added a photo({file}) on server", AuthUser.Id, "/image/profil-fotograf/" + Name);
                    string OldPhoto = AuthUser.PictureUrl;
                    AuthUser.PictureUrl = "/image/profil-fotograf/" + Name;
                    AuthUser.LastChangedOn = DateTime.Now;
                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {
                        await SignInWithCookie(AuthUser);
                        Logger.LogInformation("User({Id}) changed profile photo", AuthUser.Id);
                        if (OldPhoto != null)
                        {
                            DeleteFileFromServer(OldPhoto);
                        }
                    }
                    else
                    {
                        Logger.LogError("Changes aren't save, User({Id}) take error when trying change profile photo", AuthUser.Id);
                        DeleteFileFromServer("/image/profil-fotograf/" + Name);
                        TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                    }
                }
                else
                {
                    TempData["ProfileMessage"] = "PNG, JPG ve JPEG türünde fotoğraf yükleyiniz";
                }
            }
            else if (File.Length > 1048576)
            {
                TempData["ProfileMessage"] = "Seçtiğiniz dosya 1 megabyte'dan fazla";
            }

            Response.Redirect("/" + AuthUser.Username);
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/remove-photo")]
        public async Task RemovePhoto()
        {

            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthUser.PictureUrl != null)
            {
                string OldPhoto = AuthUser.PictureUrl;
                AuthUser.PictureUrl = null;
                AuthUser.LastChangedOn = DateTime.Now;
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    await SignInWithCookie(AuthUser);
                    Logger.LogInformation("User({Id}) removed their photo", AuthUser.Id);
                    DeleteFileFromServer(OldPhoto);
                }
                else
                {
                    Logger.LogError("Changes aren't save, User({Id}) take error when trying remove their photo", AuthUser.Id);
                    TempData["ProfileMessage"] = "Başaramadık, ne olduğunu bilmiyoruz";
                }
            }

            Response.Redirect("/" + AuthUser.Username);
        }
        #endregion
    }
}