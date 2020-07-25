using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data.Model;
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
        [Route("account/education-document")]
        public async Task SendEducationDocument(long confirmEducationId, IFormFile File)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == confirmEducationId && !x.IsRemoved && !x.IsSentToConfirmation);
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
                                UserEducationId = confirmEducationId,
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

        public async Task<IActionResult> EditView([FromQuery] EducationModel educationModel)
        {
            educationModel.Universities = await Context.University.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() }).ToListAsync();
            educationModel.ListYears();
            return View(educationModel);
        }
    }
}
