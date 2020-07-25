using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using System;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    //api/education/{actionroutename}
    public class EducationController : ApiController
    {
        [Authorize, HttpGet("get")]
        public async Task<IActionResult> Get(long EducationId) //get education informations
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();
            var AuthenticatedUserId = User.Identity.GetUserId();
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == EducationId && !x.IsRemoved && x.UserId == long.Parse(AuthenticatedUserId));

            if (Education != null)
            {
                var educationModel = new EducationModel
                {
                    UniversityId = Education.UniversityId,
                    Department = Education.Department,
                    EducationId = EducationId,
                    ActivitiesSocieties = Education.ActivitiesSocieties,
                    Startyear = int.Parse(Education.StartYear),
                    Finishyear = int.Parse(Education.EndYear),
                    
                };
                educationModel.AreUniversityorDepartmentCanEditable = Education.AreUniversityorDepartmentCanEditable();
                if (educationModel.ActivitiesSocieties == null || educationModel.ActivitiesSocieties == "undefined")
                {
                    educationModel.ActivitiesSocieties = "";
                }

                jsonReturnModel.Data = educationModel;
            }
            else
            {
                jsonReturnModel.Code = 404;
                return Error(jsonReturnModel);
            }

            return Succes(jsonReturnModel);
        }

        [HttpPost("post"), Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Post(EducationModel Model)
        {
            var AuthenticatedUserId = User.Identity.GetUserId();
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (Model.Startyear > Model.Finishyear)
            {
                jsonReturnModel.Message = "Başlangıç yılı, bitiş yılından büyük olamaz";
                jsonReturnModel.Code = 200;
                return Error(jsonReturnModel);
            }
            else if (Model.Startyear < 1980 || Model.Startyear > DateTime.Now.Year || Model.Finishyear < 1980 || Model.Startyear > DateTime.Now.Year + 7)
            {
                jsonReturnModel.Message = "Başlangıç yılı, bitiş yılı ile alakalı bilgileri kontrol edip, tekrar deneyin";
                jsonReturnModel.Code = 200;
                return Error(jsonReturnModel);
            }

            if (Model.EducationId > 0 && !string.IsNullOrEmpty(Model.EducationId.ToString()))
            {
                var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Model.EducationId && !x.IsRemoved && long.Parse(AuthenticatedUserId) == x.UserId);

                if (Education != null)
                {
                    Education.StartYear = Model.Startyear.ToString();
                    Education.EndYear = Model.Finishyear.ToString();
                    Education.ActivitiesSocieties = Model.ActivitiesSocieties;

                    if (Education.AreUniversityorDepartmentCanEditable())
                    {
                        Education.UniversityId = Model.UniversityId;
                        Education.Department = Model.Department;
                    }
                }
                else
                {
                    jsonReturnModel.Code = 404;
                    return Error(jsonReturnModel);
                }
            }
            else
            {
                var NewEducation = new UserEducation
                {
                    UserId = long.Parse(AuthenticatedUserId),
                    UniversityId = Model.UniversityId,
                    Department = Model.Department,
                    ActivitiesSocieties = Model.ActivitiesSocieties,
                    StartYear = Model.Startyear.ToString(),
                    EndYear = Model.Finishyear.ToString(),
                };
                await Context.AddAsync(NewEducation);
            }
            try
            {
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    jsonReturnModel.Message = "Eğitim bilgisi kaydedildi";
                    return Succes(jsonReturnModel);
                }
                else
                {
                    jsonReturnModel.Message = "Bir işlem yapılmadı";
                    jsonReturnModel.Code = 1001;
                    return Error(jsonReturnModel);
                }
            }
            catch (Exception e)
            {
                jsonReturnModel.Code = 200;

                string innerMessage = (e.InnerException != null) ? e.InnerException.Message.ToLower() : "";

                if (innerMessage.Contains("department"))
                {
                    jsonReturnModel.Message = "Bölüm bilgilerine ulaşamadık veya eksik";
                }
                else if (innerMessage.Contains("university"))
                {
                    jsonReturnModel.Message = "Üniversite bilgilerine ulaşamadık veya eksik";
                }
                else if (innerMessage.Contains("startyear") || innerMessage.Contains("endyear"))
                {
                    jsonReturnModel.Message = "Başlangıç veya bitiş yılını kontrol edin";
                }
                else
                {
                    jsonReturnModel.Message = "Başaramadık ve ne olduğunu bilmiyoruz, tekrar deneyin";
                }

                return Error(jsonReturnModel);
            }
        }
    }
}
