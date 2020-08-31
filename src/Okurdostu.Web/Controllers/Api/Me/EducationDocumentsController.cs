using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Models;
using Okurdostu.Web.Services;
using System;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api.Me
{
    [Route("api/me/educationdocuments")]
    public class EducationDocumentsController : SecureApiController
    {
        private readonly ILocalStorageService LocalStorage;
        private readonly IHostingEnvironment Environment;

        public EducationDocumentsController(IHostingEnvironment hostingEnvironment, ILocalStorageService localStorageService)
        {
            LocalStorage = localStorageService;
            Environment = hostingEnvironment;
        }

        [HttpGet("")]
        public ActionResult Index()
        {
            return NotFound();
        }

        [HttpPost("")] //api/me/educationdocuments
        public async Task<IActionResult> Post(Guid Id, IFormFile File)
        {
            var UploadingStatus = LocalStorage.UploadEducationDocument(File, Environment.WebRootPath);
            if (!UploadingStatus.Succes)
            {
                return Error(UploadingStatus.Message);
            }

            var AuthenticatedUserId = Guid.Parse(User.Identity.GetUserId());

            var Education = await Context.UserEducation.FirstOrDefaultAsync(
               x => x.Id == Id
            && x.UserId == AuthenticatedUserId
            && !x.IsRemoved
            && !x.IsSentToConfirmation);

            if (Education != null)
            {
                var EducationDocument = new UserEducationDoc
                {
                    UserEducationId = Id,
                    FullPath = Environment.WebRootPath + UploadingStatus.UploadedPathAfterRoot,
                    PathAfterRoot = UploadingStatus.UploadedPathAfterRoot,
                };

                Education.IsSentToConfirmation = true;
                await Context.AddAsync(EducationDocument);
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    TempData["ProfileMessage"] = "Eğitim dökümanınız yollandı, en geç 48 saat içinde geri dönüş yapılacak";
                    return Succes("Eğitim dökümanınız yollandı", null, 201);
                }
                else
                {
                    LocalStorage.DeleteIfExists(EducationDocument.FullPath);
                    return Error(null, null, null, 1001);
                }
            }

            return Error("Eğitim bilgisine ulaşılamadı", null, null, 404);
        }
    }
}