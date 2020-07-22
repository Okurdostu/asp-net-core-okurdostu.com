using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data.Model;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    [Authorize]
    public class EducationController : BaseController<EducationController>
    {

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

        User AuthUser;

#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IHostingEnvironment Environment;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
        public EducationController(IHostingEnvironment env) => Environment = env;
#pragma warning restore CS0618 // Type or member is obsolete


        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/education")]
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
                {
                    TempData["ProfileMessage"] = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı";
                }
            }
            else
            {
                TempData["ProfileMessage"] = "Böyle bir üniversite yok";
            }

            Response.Redirect("/" + AuthUser.Username);
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/edit-education")]
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

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/remove-education")]
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

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("account/education-document")]
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

                        string DocumentName = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName);
                        string FilePathWithName = Environment.WebRootPath + "/documents/" + DocumentName;

                        using (var Stream = System.IO.File.Create(FilePathWithName))
                        {
                            await File.CopyToAsync(Stream);
                        };
                        Logger.LogInformation("User({Id}) uploaded a education document({File}) on server", AuthUser.Id, DocumentName);

                        if (System.IO.File.Exists(FilePathWithName))
                        {

                            var EducationDocument = new UserEducationDoc
                            {
                                CreatedOn = DateTime.Now,
                                UserEducationId = Id,
                                FullPath = FilePathWithName,
                                PathAfterRoot = "/documents/" + DocumentName,
                            };
                            Education.IsSentToConfirmation = true;

                            await Context.AddAsync(EducationDocument);
                            var result = await Context.SaveChangesAsync();

                            if (result > 0)
                            {
                                Logger.LogInformation("Database seeded for a education document({File}).", DocumentName);
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
                    {
                        TempData["ProfileMessage"] = "PDF, PNG, JPG veya JPEG türünde belge yükleyiniz";
                    }

                }
                else if (File.Length > 1048576)
                {
                    TempData["ProfileMessage"] = "Seçtiğiniz dosya 1 megabyte'dan fazla olmamalı";
                }
                else
                {
                    TempData["ProfileMessage"] = "Seçtiğiniz dosya ile alakalı problemler var";
                }
            }

            Response.Redirect("/" + AuthUser.Username);
        }
    }
}
