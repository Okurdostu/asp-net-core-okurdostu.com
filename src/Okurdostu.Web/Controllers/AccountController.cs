using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Okurdostu.Web.Controllers
{
    [Authorize]
    public class AccountController : OkurdostuContextController
    {
        private User AuthUser;

#pragma warning disable CS0618 // Type or member is obsolete
        private IHostingEnvironment Environment;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
        public AccountController(IHostingEnvironment env) => Environment = env;
#pragma warning restore CS0618 // Type or member is obsolete

        #region account
        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/fotograf")]
        public async Task AddPhoto()
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            var File = Request.Form.Files.First();

            if (File.Length < 1048576)
            {
                if (File.ContentType == "image/png" || File.ContentType == "image/jpg" || File.ContentType == "image/jpeg")
                {
                    string NewName = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName);
                    string FilePathWithName = Environment.WebRootPath + "/image/profil-fotograf/" + NewName;
                    using var image = Image.Load(File.OpenReadStream());
                    if (image.Width > 200)
                        image.Mutate(x => x.Resize(200, 200));
                    image.Save(FilePathWithName);
                    AuthUser.PictureUrl = "/image/profil-fotograf/" + NewName;
                    await Context.SaveChangesAsync();
                }
                else
                    TempData["ProfileMessage"] = "PNG, JPG ve JPEG türünde resim yükleyiniz";

            }
            else
                TempData["ProfileMessage"] = "Seçtiğiniz dosya 1 megabyte'dan fazla";

            Response.Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/fotograf-kaldir")]
        public async Task RemovePhoto()
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            AuthUser.PictureUrl = null;
            await Context.SaveChangesAsync();
            Response.Redirect("/" + AuthUser.Username);
        }
        #endregion

        #region Education
        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/egitim")]
        public async Task AddEducation(EducationModel Model)
        {
            var University = await Context.University.FirstOrDefaultAsync(x => x.Id == Model.UniversityId);
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (University != null)
            {
                if (Model.Startyear < Model.Finishyear)
                {
                    if (User != null)
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
                            TempData["ProfileMessage"] = "Eğitim bilginiz eklendi<br />Onaylanması için belge yollamayı unutmayın.";
                        else
                            TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                    }
                }
                else
                    TempData["ProfileMessage"] = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı";
            }
            Response.Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/egitim-duzenle")]
        public async Task EditEducation(EducationModel Model)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Model.EducationId);
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (Education != null)
            {
                if (Education.UserId == AuthUser.Id)
                {
                    Education.ActivitiesSocieties = Model.ActivitiesSocieties;
                    if (Education.IsUniversityInformationsCanEditable())
                    {
                        Education.UniversityId = (short)Model.UniversityId;
                        Education.Department = Model.Department;
                    }
                    if (Model.Startyear < Model.Finishyear)
                    {
                        Education.StartYear = Model.Startyear.ToString();
                        Education.EndYear = Model.Finishyear.ToString();
                        TempData["ProfileMessage"] = "Eğitim bilgileriniz düzenlendi";
                    }
                    else
                        TempData["ProfileMessage"] = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı" +
                            "<br />" + "Bunlar dışında ki eğitim bilgileriniz düzenlendi";

                    var result = await Context.SaveChangesAsync();
                    if (result! > 0)
                        TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                }
                else
                    TempData["ProfileMessage"] = "MC Hammer: You can't touch this";
            }
            Response.Redirect("/" + AuthUser.Username);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/egitim-kaldir")]
        public async Task RemoveEducation(long Id, string Username)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && x.User.Username == Username && !x.IsRemoved);
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (Education != null)
            {
                if (AuthUser.Id == Education.UserId)
                {
                    var AuthenticatedUserNeedCount = Context.Need.Where(x => !x.IsRemoved && x.UserId == AuthUser.Id).Count();
                    if (Education.IsActiveEducation && AuthenticatedUserNeedCount > 0)
                        TempData["ProfileMessage"] = "İhtiyaç kampanyanız olduğu için" +
                            "<br />" +
                            "Aktif olan eğitim bilginizi silemezsiniz." +
                            "Aktif olan eğitim bilgisi, hala burada okuduğunuzu iddia ettiğiniz bir eğitim bilgisidir.";
                    else
                    {
                        Education.IsRemoved = true;
                        var result = await Context.SaveChangesAsync();
                        if (result! > 0)
                            TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                    }
                }
                else
                    TempData["ProfileMessage"] = "MC Hammer: You can't touch this";
            }

            Response.Redirect("/" + AuthUser.Username);
        }
        #endregion
    }
}
