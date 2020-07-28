using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api.Me
{
    [Route("api/me/educationdocuments")]
    public class EducationDocumentsController : SecureApiController
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IHostingEnvironment Environment;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
        public EducationDocumentsController(IHostingEnvironment env) => Environment = env;
#pragma warning restore CS0618 // Type or member is obsolete

        [NonAction]
        public bool DeleteFileFromServer(string filePathAfterRootPath)
        {
            if (System.IO.File.Exists(Environment.WebRootPath + filePathAfterRootPath))
            {
                System.IO.File.Delete(Environment.WebRootPath + filePathAfterRootPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpPost("")] //api/me/educationdocuments
        public async Task<IActionResult> Post(Guid Id, IFormFile File)
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (File != null && File.Length <= 10485767 / 2 && File.Length > 0)
            {
                if (File.ContentType != "application/pdf" && File.ContentType != "image/png" && File.ContentType != "image/jpg" && File.ContentType != "image/jpeg")
                {
                    jsonReturnModel.Code = 200;
                    jsonReturnModel.Message = "PDF, PNG, JPG veya JPEG türünde dosya yollayabilirsin";
                    return Error(jsonReturnModel);
                }
            }
            else if (File != null && File.Length > 10485767 / 2)
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "En fazla 500 kilobyte boyutunda dosya yollayabilirsin";
                return Error(jsonReturnModel);
            }
            else if (File != null && File.Length! > 0)
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "Yolladığınız dosya görüntülenemez";
                return Error(jsonReturnModel);
            }
            else // file is null
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "Dosya yollamadınız";
                return Error(jsonReturnModel);
            }

            var AuthenticatedUserId = Guid.Parse(User.Identity.GetUserId());

            var Education = await Context.UserEducation.FirstOrDefaultAsync(
               x => x.Id == Id
            && x.UserId == AuthenticatedUserId
            && !x.IsRemoved
            && !x.IsSentToConfirmation);

            if (Education != null)
            {
                string DocumentName = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName);
                string FileFullPath = Environment.WebRootPath + "/documents/" + DocumentName;

                using (var Stream = System.IO.File.Create(FileFullPath))
                {
                    await File.CopyToAsync(Stream);
                };

                if (System.IO.File.Exists(FileFullPath))
                {
                    var EducationDocument = new UserEducationDoc
                    {
                        CreatedOn = DateTime.Now,
                        UserEducationId = Id,
                        FullPath = FileFullPath, // C:/application/wwwroot/documents/{guid}.extensiontype
                        PathAfterRoot = "/documents/" + DocumentName, // /documents/{guid}.extensiontype
                    };

                    Education.IsSentToConfirmation = true;

                    await Context.AddAsync(EducationDocument);

                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {
                        jsonReturnModel.Message = "Eğitim dökümanınız teslim alındı";
                        TempData["ProfileMessage"] = "Eğitim dökümanınız yollandı, en geç 48 saat içinde geri dönüş yapılacak";
                        return Succes(jsonReturnModel);
                    }
                    else
                    {
                        DeleteFileFromServer(EducationDocument.PathAfterRoot);
                        return Error(jsonReturnModel);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Logger.LogCritical("A user is trying to send education document but file wasn't create on server");
                    }
                    return Error(jsonReturnModel);
                }
            }

            jsonReturnModel.Code = 404;
            return Error(jsonReturnModel);
        }
    }
}