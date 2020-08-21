using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
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
            ReturnModel rm = new ReturnModel();
            string innerMessage = (e.InnerException != null) ? e.InnerException.Message.ToLower() : "";

            if (innerMessage.Contains("university"))
            {
                rm.Message = "Üniversite bilgilerine ulaşamadık veya eksik";
            }
            else if (innerMessage.Contains("department"))
            {
                rm.Message = "Bölüm bilgilerine ulaşamadık veya eksik";
            }
            else if (innerMessage.Contains("startyear") || innerMessage.Contains("endyear"))
            {
                rm.Message = "Başlangıç veya bitiş yılını kontrol edin";
            }
            else
            {
                rm.Message = "Başaramadık ve ne olduğunu bilmiyoruz, tekrar deneyin";
            }

            return Error(rm);
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost("")]
        public async Task<IActionResult> PostAdd(EducationModel Model)
        {
            var AuthenticatedUserId = User.Identity.GetUserId();
            ReturnModel rm = new ReturnModel();

            if (Model.StartYear > Model.EndYear)
            {
                rm.Message = "Başlangıç yılı, bitiş yılından büyük olamaz";
                return Error(rm);
            }

            if (Model.StartYear < 1980 || Model.StartYear > DateTime.Now.Year || Model.EndYear < 1980 || Model.StartYear > DateTime.Now.Year + 7)
            {
                rm.Message = "Başlangıç yılı, bitiş yılı ile alakalı bilgileri kontrol edip, tekrar deneyin";
                return Error(rm);
            }

            var NewEducation = new UserEducation
            {
                UserId = Guid.Parse(AuthenticatedUserId),
                UniversityId = Model.UniversityId,
                Department = Model.Department.ClearBlanks(),
                ActivitiesSocieties = Model.ActivitiesSocieties.ClearBlanks(),
                StartYear = Model.StartYear.ToString(),
                EndYear = Model.EndYear.ToString(),
            };
            await Context.AddAsync(NewEducation);
            try
            {
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    rm.Code = 201;
                    rm.Message = "Eğitim bilgisi kaydedildi";
                    return Succes(rm);
                }
                else
                {
                    rm.Code = 1001;
                    return Error(rm);
                }
            }
            catch (Exception e)
            {
                return ReturnByInnerMessage(e);
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> GetEducations() //need: pagination
        {
            ReturnModel rm = new ReturnModel();

            var Educations = await Context.UserEducation.Where(x => x.UserId == Guid.Parse(User.Identity.GetUserId())).Select(x => new
            {
                x.Id,
                x.IsRemoved,
                x.StartYear,
                x.EndYear,
                x.IsConfirmed,
                x.IsActiveEducation,
                x.IsSentToConfirmation,

                universityPageUrl = "/universite/" + x.University.FriendlyName,
                universityLogoUrl = x.University.LogoUrl,
                universityName = x.University.Name,
                universityId = x.UniversityId,
            }).ToListAsync();

            if (Educations != null)
            {
                rm.Data = Educations;
                return Succes(rm);
            }

            rm.Code = 404;
            return Error(rm);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetSingle(Guid Id)
        {
            ReturnModel rm = new ReturnModel();
            var AuthenticatedUserId = User.Identity.GetUserId();
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && x.UserId == Guid.Parse(AuthenticatedUserId));

            if (Education != null)
            {
                var educationModel = new EducationModel
                {
                    UniversityId = Education.UniversityId,
                    Department = Education.Department,
                    EducationId = Id,
                    ActivitiesSocieties = Education.ActivitiesSocieties,
                    StartYear = int.Parse(Education.StartYear),
                    EndYear = int.Parse(Education.EndYear),
                };

                educationModel.AreUniversityorDepartmentCanEditable = Education.AreUniversityorDepartmentCanEditable();
                if (educationModel.ActivitiesSocieties == null || educationModel.ActivitiesSocieties == "undefined")
                {
                    educationModel.ActivitiesSocieties = "";
                }

                rm.Data = educationModel;
            }
            else
            {
                return Error(rm);
            }
            return Succes(rm);
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
            ReturnModel rm = new ReturnModel();

            if (!ModelState.IsValid)
            {
                rm.Message = "Silinmesi gereken eğitim bilgisine ulaşılamadı";
                rm.InternalMessage = "Id is required";
                return Error(rm);
            }

            var deletedEducation = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && Guid.Parse(AuthenticatedUserId) == x.UserId);

            if (deletedEducation != null)
            {
                if (await IsCanRemovable(deletedEducation).ConfigureAwait(false))
                {
                    deletedEducation.IsRemoved = true;
                    var result = await Context.SaveChangesAsync();

                    if (result > 0)
                    {
                        rm.Message = "Eğitim bilgisi kaldırıldı";
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
                        return Succes(rm);
                    }
                    else
                    {
                        rm.Code = 1001;
                        return Error(rm);
                    }
                }
                else
                {
                    rm.Message = "Bu eğitimi silemezsiniz";

                    TempData["ProfileMessage"] = "İhtiyaç kampanyanız olduğu için" +
                        "<br />" +
                        "Aktif olan eğitim bilginizi silemezsiniz." +
                        "<br />" +
                        "Aktif olan eğitim bilgisi, belge yollayarak hala burada okuduğunuzu iddia ettiğiniz bir eğitim bilgisidir." +
                        "<br/>" +
                        "Daha fazla ayrıntı ve işlem için: info@okurdostu.com";

                    return Error(rm);
                }
            }
            else
            {
                rm.Code = 404;
                rm.Message = "Böyle bir eğitiminiz yok";
                rm.InternalMessage = "Education is null";
                return Error(rm);
            }
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> PutEdit(Guid Id, EducationModel Model)
        {
            var AuthenticatedUserId = User.Identity.GetUserId();
            ReturnModel rm = new ReturnModel();
            var editedEducation = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && Guid.Parse(AuthenticatedUserId) == x.UserId);

            if (Model.StartYear > Model.EndYear)
            {
                rm.Message = "Başlangıç yılı, bitiş yılından büyük olamaz";
                return Error(rm);
            }
            else if (Model.StartYear < 1980 || Model.StartYear > DateTime.Now.Year || Model.EndYear < 1980 || Model.StartYear > DateTime.Now.Year + 7)
            {
                rm.Message = "Başlangıç yılı, bitiş yılı ile alakalı bilgileri kontrol edip, tekrar deneyin";
                return Error(rm);
            }

            if (editedEducation != null)
            {
                editedEducation.StartYear = Model.StartYear.ToString();
                editedEducation.EndYear = Model.EndYear.ToString();
                editedEducation.ActivitiesSocieties = Model.ActivitiesSocieties.ClearBlanks();

                if (editedEducation.AreUniversityorDepartmentCanEditable())
                {
                    editedEducation.UniversityId = Model.UniversityId;
                    editedEducation.Department = Model.Department.ClearBlanks();
                }
            }
            else
            {
                rm.Message = "Böyle bir eğitiminiz yok";
                rm.InternalMessage = "Education is null";
                rm.Code = 404;
                return Error(rm);
            }

            try
            {
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    rm.Message = "Eğitim bilgisi kaydedildi";
                    return Succes(rm);
                }
                else
                {
                    rm.Code = 1001;
                    return Error(rm);
                }
            }
            catch (Exception e)
            {
                return ReturnByInnerMessage(e);
            }
        }
    }
}
