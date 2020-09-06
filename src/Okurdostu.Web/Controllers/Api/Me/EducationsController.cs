using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api.Me
{
    [Route("api/me/educations")]
    public class EducationsController : SecureApiController
    {
        private readonly ILocalStorageService LocalStorage;

        public EducationsController(ILocalStorageService localStorageService)
        {
            LocalStorage = localStorageService;
        }

        [NonAction]
        public IActionResult ReturnByInnerMessage(Exception e) //return according to innermessage when operation drops into the catch while adding or editing a education
        {
            string innerMessage = (e.InnerException != null) ? e.InnerException.Message.ToLower() : "";
            if (innerMessage.Contains("university"))
                return Error("Üniversite bilgilerine ulaşamadık veya eksik");
            else if (innerMessage.Contains("department"))
                return Error("Bölüm bilgilerine ulaşamadık veya eksik");
            else if (innerMessage.Contains("startyear") || innerMessage.Contains("endyear"))
                return Error("Başlangıç veya bitiş yılını kontrol edin");
            else
                return Error("Başaramadık ve ne olduğunu bilmiyoruz, tekrar deneyin");
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost("")]
        public async Task<IActionResult> PostAdd(EducationModel Model)
        {
            var AuthenticatedUserId = User.Identity.GetUserId();

            if (Model.StartYear > Model.EndYear)
            {
                return Error("Başlangıç yılı, bitiş yılından büyük olamaz");
            }

            if (Model.StartYear < 1980 || Model.StartYear > DateTime.Now.Year || Model.EndYear < 1980 || Model.StartYear > DateTime.Now.Year + 7)
            {
                return Error("Başlangıç yılı, bitiş yılı ile alakalı bilgileri kontrol edip, tekrar deneyin");
            }

            var NewEducation = new UserEducation
            {
                UserId = Guid.Parse(AuthenticatedUserId),
                UniversityId = Model.UniversityId,
                Department = Model.Department.ClearExtraBlanks().CapitalizeFirstCharOfWords().RemoveLessGreaterSigns(),
                ActivitiesSocieties = Model.ActivitiesSocieties.ClearExtraBlanks().RemoveLessGreaterSigns(),
                StartYear = Model.StartYear.ToString().RemoveLessGreaterSigns(),
                EndYear = Model.EndYear.ToString().RemoveLessGreaterSigns(),
            };
            await Context.AddAsync(NewEducation);
            try
            {
                await Context.SaveChangesAsync();
                return Succes("Eğitim bilgisi kaydedildi", null, 201);
            }
            catch (Exception e)
            {
                return ReturnByInnerMessage(e);
            }
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetSingle(Guid Id)
        {
            var AuthenticatedUserId = User.Identity.GetUserId();
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && x.UserId == Guid.Parse(AuthenticatedUserId));

            if (Education != null)
            {
                var education = new EducationModel
                {
                    UniversityId = Education.UniversityId,
                    Department = Education.Department,
                    EducationId = Id,
                    ActivitiesSocieties = Education.ActivitiesSocieties,
                    StartYear = int.Parse(Education.StartYear),
                    EndYear = int.Parse(Education.EndYear),
                };

                education.AreUniversityorDepartmentCanEditable = Education.AreUniNameOrDepartmentCanEditable();
                if (education.ActivitiesSocieties == null || education.ActivitiesSocieties == "undefined")
                {
                    education.ActivitiesSocieties = "";
                }

                return Succes(null, education);
            }

            return Error(null, null, null, 404);
        }
        public async Task<bool> IsCanRemovable(UserEducation edu)
        {
            if (edu.IsConfirmed || edu.IsActiveEducation)
            {
                var EducationUserAnyActiveNeed = await Context.Need.AnyAsync(x => !x.IsRemoved && x.UserId == edu.UserId);
                return !EducationUserAnyActiveNeed;
            }
            return true;
        }

        [HttpPatch("{Id}")]
        public async Task<IActionResult> PatchRemove(Guid Id)
        {
            var AuthenticatedUserId = User.Identity.GetUserId();
            if (Id == Guid.Empty)
            {
                return Error("Silinmesi gereken eğitim bilgisine ulaşılamadı");
            }

            var deletedEducation = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && Guid.Parse(AuthenticatedUserId) == x.UserId);
            if (deletedEducation != null)
            {
                if (await IsCanRemovable(deletedEducation))
                {
                    deletedEducation.IsRemoved = true;
                    await Context.SaveChangesAsync();

                    if (deletedEducation.IsSentToConfirmation)
                    {
                        var educationDocuments = await Context.UserEducationDoc.Where(x => x.UserEducationId == deletedEducation.Id).ToListAsync();
                        foreach (var item in educationDocuments)
                        {
                            if (LocalStorage.DeleteIfExists(item.FullPath))
                            {
                                Context.Remove(item);
                            }
                        }
                        await Context.SaveChangesAsync();
                    }
                    return Succes("Eğitim bilgisi kaldırıldı");
                }
                else
                {
                    TempData["ProfileMessage"] = "İhtiyaç kampanyanız olduğu için" +
                        "<br />" +
                        "Aktif olan eğitim bilginizi silemezsiniz." +
                        "<br />" +
                        "Aktif olan eğitim bilgisi, belge yollayarak hala burada okuduğunuzu iddia ettiğiniz bir eğitim bilgisidir." +
                        "<br/>" +
                        "Daha fazla ayrıntı ve işlem için: info@okurdostu.com";

                    return Error("Bu eğitimi silemezsiniz");
                }
            }

            return Error("Böyle bir eğitiminiz yok", null, null, 404);
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> PutEdit(Guid Id, EducationModel Model)
        {
            var AuthenticatedUserId = User.Identity.GetUserId();
            var editedEducation = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && Guid.Parse(AuthenticatedUserId) == x.UserId);

            if (Model.StartYear > Model.EndYear)
                return Error("Başlangıç yılı, bitiş yılından büyük olamaz");
            else if (Model.StartYear < 1980 || Model.StartYear > DateTime.Now.Year || Model.EndYear < 1980 || Model.StartYear > DateTime.Now.Year + 7)
                return Error("Başlangıç yılı, bitiş yılı ile alakalı bilgileri kontrol edip, tekrar deneyin");

            if (editedEducation != null)
            {
                editedEducation.StartYear = Model.StartYear.ToString().RemoveLessGreaterSigns();
                editedEducation.EndYear = Model.EndYear.ToString().RemoveLessGreaterSigns();
                editedEducation.ActivitiesSocieties = Model.ActivitiesSocieties.ClearExtraBlanks().RemoveLessGreaterSigns();

                if (editedEducation.AreUniNameOrDepartmentCanEditable())
                {
                    editedEducation.UniversityId = Model.UniversityId;
                    editedEducation.Department = Model.Department.ClearExtraBlanks().CapitalizeFirstCharOfWords().RemoveLessGreaterSigns();
                }
            }
            else
            {
                return Error("Böyle bir eğitiminiz yok", null, null, 404);
            }

            try
            {
                await Context.SaveChangesAsync();
                return Succes("Eğitim bilgisi kaydedildi", null, 201);
            }
            catch (Exception e)
            {
                return ReturnByInnerMessage(e);
            }
        }
    }
}