using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data.Model;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
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
        private IHostingEnvironment Environment;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
        public AccountController(IHostingEnvironment env) => Environment = env;
#pragma warning restore CS0618 // Type or member is obsolete

        #region account
        [NonAction]
        public async Task<bool> ConfirmIdentityWithPassword(string ConfirmPassword)
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            ConfirmPassword = ConfirmPassword.SHA512();
            return ConfirmPassword == AuthUser.Password ? true : false;
        }


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


        //  email editleme:

        //  1.  Guid ile bir key oluşturulacak ve kullanıcı ile bu key eşleştirilip veritabanına alınacak
        //  2.  Keye ulaşabileceği bir link o an kullandığı e-mail adresine yollanacak
        //  3.  O keyli link kullanılarak yeni bir e-mail adresi girişi yapabilecek
        //  4.  Yeni girişin yapıldığı e-mail ilk başta veritabanında eşleştirdiğimiz(key ve kullanıcı) kullanıcıya atanıp: yeni e-mail için onay istenecek

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/password")]
        public async Task EditPassword(ProfileModel Model)
        {
            if (await ConfirmIdentityWithPassword(Model.ConfirmPassword))
            {
                if (Model.RePassword == Model.Password)
                {
                    Model.Password = Model.Password.SHA512();
                    if (AuthUser.Password != Model.Password)
                    {
                        AuthUser.Password = Model.Password;
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
                    TempData["ProfileMessage"] = "Yeni parolalarınız birbiri ile eşleşmedi";
                }
            }
            else
            {
                TempData["ProfileMessage"] = "Kimliğinizi doğrulayamadık";
            }

            Response.Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/username")]
        public async Task EditUsername(ProfileModel Model)
        {
            if (await ConfirmIdentityWithPassword(Model.ConfirmPassword))
            {
                Model.Username = Model.Username.ToLower();
                if (AuthUser.Username != Model.Username)
                {
                    string NowUsername = AuthUser.Username;
                    AuthUser.Username = Model.Username;
                    try
                    {
                        await Context.SaveChangesAsync();
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
                TempData["ProfileMessage"] = "Kimliğinizi doğrulayamadık";
            }

            Response.Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/contact")]
        public async Task Contact(ProfileModel Model) //editing, adding contacts
        {

            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthUser.ContactEmail != Model.ContactEmail || AuthUser.Twitter != Model.Twitter || AuthUser.Github != Model.Github)
            {
                AuthUser.ContactEmail = Model.ContactEmail;
                AuthUser.Twitter = Model.Twitter;
                AuthUser.Github = Model.Github;
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

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/basic")]
        public async Task ProfileBasic(ProfileModel Model) //editing, adding bio and fullname
        {

            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthUser.Biography != Model.Biography || AuthUser.FullName != Model.FullName)
            {
                AuthUser.Biography = Model.Biography;
                AuthUser.FullName = Model.FullName;
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

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/photo")]
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
                        ImageSharp.Mutate(x => x.Resize(200, 200));

                    ImageSharp.Save(FilePathWithName);
                    Logger.LogInformation("User({Id}) added a photo({file}) on server", AuthUser.Id, "/image/profil-fotograf/" + Name);
                    string OldPhoto = AuthUser.PictureUrl;
                    AuthUser.PictureUrl = "/image/profil-fotograf/" + Name;

                    var result = await Context.SaveChangesAsync();

                    if (result > 0)
                    {
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
                    TempData["ProfileMessage"] = "PNG, JPG ve JPEG türünde fotoğraf yükleyiniz";
            }
            else if (File.Length > 1048576)
                TempData["ProfileMessage"] = "Seçtiğiniz dosya 1 megabyte'dan fazla";

            Response.Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/remove-photo")]
        public async Task RemovePhoto()
        {

            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (AuthUser.PictureUrl != null)
            {
                string OldPhoto = AuthUser.PictureUrl;
                AuthUser.PictureUrl = null;
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
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


        #region Education
        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/education")]
        public async Task AddEducation(EducationModel Model)
        {
            var University = await Context.University.FirstOrDefaultAsync(x => x.Id == Model.UniversityId);
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (University != null)
            {
                if (Model.Startyear <= Model.Finishyear)
                {
                    var Education = new UserEducation
                    {
                        UserId = AuthUser.Id,
                        UniversityId = University.Id,
                        Department = Model.Department,
                        StartYear = Model.Startyear.ToString(),
                        EndYear = Model.Finishyear.ToString(),
                        ActivitiesSocieties = Model.ActivitiesSocieties
                    };

                    await Context.UserEducation.AddAsync(Education);
                    var result = await Context.SaveChangesAsync();

                    if (result > 0)
                    {
                        Logger.LogInformation("User({Id}) added a new education information", AuthUser.Id);
                        TempData["ProfileMessage"] = "Eğitim bilginiz eklendi<br />Onaylanması için belge yollamayı unutmayın.";
                    }
                    else
                    {
                        Logger.LogError("Changes aren't save, User({Id}) take error when trying add a new education information", AuthUser.Id);
                        TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                    }

                }
                else
                    TempData["ProfileMessage"] = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı";
            }
            else
                TempData["ProfileMessage"] = "Böyle bir üniversite yok";

            Response.Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/edit-education")]
        public async Task EditEducation(EducationModel Model)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Model.EducationId && !x.IsRemoved);
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (Education != null)
            {
                if (Education.UserId == AuthUser.Id)
                {
                    if (Education.IsUniversityInformationsCanEditable())
                    {
                        if (await Context.University.FirstOrDefaultAsync(x => x.Id == Model.UniversityId) != null)
                        {
                            Education.UniversityId = Model.UniversityId;
                            Education.Department = Model.Department;
                        }
                        else
                        {
                            TempData["ProfileMessage"] = "Böyle bir üniversite yok";
                            goto _redirect;
                        }
                    }


                    if (Model.Startyear <= Model.Finishyear)
                    {
                        Education.StartYear = Model.Startyear.ToString();
                        Education.EndYear = Model.Finishyear.ToString();
                    }
                    else
                    {
                        TempData["ProfileMessage"] = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı";
                    }

                    Education.ActivitiesSocieties = Model.ActivitiesSocieties;
                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {
                        Logger.LogInformation("User({Id}) edited an education information", AuthUser.Id);

                        if (!(Model.Startyear <= Model.Finishyear))
                        {
                            TempData["ProfileMessage"] = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı" +
                            "<br />" + "Bunlar dışında ki eğitim bilgileriniz düzenlendi";
                        }
                        else
                        {
                            TempData["ProfileMessage"] = "Eğitim bilgileriniz düzenlendi";
                        }
                    }
                }
                else
                {
                    Logger.LogWarning(401, "User({Id}) tried change another user data[Education: {EducationId}]", AuthUser.Id, Education.Id);
                    TempData["ProfileMessage"] = "MC Hammer: You can't touch this";
                }

            }
        _redirect: Response.Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/remove-education")]
        public async Task RemoveEducation(long Id, string Username)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && x.User.Username == Username && !x.IsRemoved);
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (Education != null)
            {
                if (AuthUser.Id == Education.UserId)
                {
                    var AuthUserActiveNeedCount = Context.Need.Where(x => !x.IsRemoved && x.UserId == AuthUser.Id).Count();
                    if (Education.IsActiveEducation && AuthUserActiveNeedCount > 0)
                    {
                        Logger.LogInformation("User({Id}) has tried deleted an active education", AuthUser.Id);

                        TempData["ProfileMessage"] = "İhtiyaç kampanyanız olduğu için" +
                            "<br />" +
                            "Aktif olan eğitim bilginizi silemezsiniz." +
                            "<br />" +
                            "Aktif olan eğitim bilgisi, belge yollayarak hala burada okuduğunuzu iddia ettiğiniz bir eğitim bilgisidir." +
                            "<br/>" +
                            "Daha fazla ayrıntı ve işlem için: info@okurdostu.com";
                    }
                    else
                    {

                        Education.IsRemoved = true;
                        var result = await Context.SaveChangesAsync();
                        if (result > 0)
                        {
                            TempData["ProfileMessage"] = "Eğitiminiz kaldırıldı";
                            Logger.LogInformation("User({Id}) deleted an education information", AuthUser.Id);

                            try
                            {
                                if (Education.IsSentToConfirmation)
                                {
                                    var EducationDocuments = await Context.UserEducationDoc.Where(x => x.UserEducationId == Education.Id).ToListAsync();
                                    foreach (var item in EducationDocuments)
                                    {
                                        if (DeleteFileFromServer(item.PathAfterRoot))
                                        {
                                            Context.Remove(item);
                                        }
                                    }
                                    await Context.SaveChangesAsync();
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("Deleting education documents failed, UserId: {Id}, EducationId {EduId} Ex message: {ExMessage}", AuthUser.Id, Education.Id, e.Message);
                            }

                        }
                        else
                        {
                            Logger.LogError("Changes aren't save, User({Id}) take error when trying delete Education:{EducationId}", AuthUser.Id, Education.Id);
                            TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                        }
                    }
                }
                else
                {
                    Logger.LogWarning(401, "User({Id}) tried remove another user data[Education: {EducationId}]", AuthUser.Id, Education.Id);
                    TempData["ProfileMessage"] = "MC Hammer: You can't touch this";
                }
            }

            Response.Redirect("/" + AuthUser.Username);
        }



        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/education-document")]
        public async Task SendEducationDocument(long Id, IFormFile File)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && !x.IsSentToConfirmation); //bir belge yollayabilir.
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (Education != null && AuthUser.Id == Education.UserId)
            {
                if (File != null && File.Length <= 1048576 && File.Length > 0)
                {

                    if (File.ContentType == "document/pdf" || File.ContentType == "image/png" || File.ContentType == "image/jpg" || File.ContentType == "image/jpeg")
                    {

                        string Name = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName);
                        string FilePathWithName = Environment.WebRootPath + "/documents/" + Name;

                        using (var Stream = System.IO.File.Create(FilePathWithName))
                        {
                            await File.CopyToAsync(Stream);
                        };
                        Logger.LogInformation("User({Id}) uploaded a education document({File}) on server", AuthUser.Id, Name);

                        if (System.IO.File.Exists(FilePathWithName))
                        {

                            var EducationDocument = new UserEducationDoc
                            {
                                CreatedOn = DateTime.Now,
                                UserEducationId = Id,
                                FullPath = FilePathWithName,
                                PathAfterRoot = "/documents/" + Name,
                            };
                            Education.IsSentToConfirmation = true;

                            await Context.AddAsync(EducationDocument);
                            var result = await Context.SaveChangesAsync();

                            if (result > 0)
                            {
                                Logger.LogInformation("Database seeded for a education document({File}).", Name);
                                TempData["ProfileMessage"] = "Eğitim dökümanınız yollandı, en geç 6 saat içinde geri dönüş yapılacak";
                            }
                            else
                            {
                                TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                                Logger.LogError("Changes aren't save, User({Id}) take error when trying add a document Education:{EducationId}", AuthUser.Id, Education.Id);

                                DeleteFileFromServer(EducationDocument.PathAfterRoot);
                            }

                        }

                    }
                    else
                        TempData["ProfileMessage"] = "PDF, PNG, JPG veya JPEG türünde belge yükleyiniz";

                }
                else if (File.Length > 1048576)
                    TempData["ProfileMessage"] = "Seçtiğiniz dosya 1 megabyte'dan fazla olmamalı";
                else
                    TempData["ProfileMessage"] = "Seçtiğiniz dosya ile alakalı problemler var";

            }

            Response.Redirect("/" + AuthUser.Username);
        }
    }
}
#endregion
