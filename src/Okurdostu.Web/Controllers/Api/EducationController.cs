using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    public class EducationController : ApiController
    {
        [Route("get")]
        public async Task<IActionResult> Get(long EducationId)
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();
            var AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (AuthUser == null)
            {
                jsonReturnModel.Code = 401;
                return Error(jsonReturnModel);
            }

            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == EducationId && !x.IsRemoved);

            if (Education != null)
            {

                if (AuthUser.Id != Education.UserId)
                {
                    jsonReturnModel.Code = 403;
                    return Error(jsonReturnModel);
                }

                var educationModel = new EducationModel
                {
                    EducationId = EducationId,
                    ActivitiesSocieties = Education.ActivitiesSocieties,
                    Startyear = int.Parse(Education.StartYear),
                    Finishyear = int.Parse(Education.EndYear),
                };

                if (educationModel.ActivitiesSocieties == null)
                {
                    educationModel.ActivitiesSocieties = "";
                }

                if (Education.IsUniversityInformationsCanEditable())
                {
                    educationModel.UniversityId = Education.UniversityId;
                    educationModel.Department = Education.Department;
                }

                jsonReturnModel.Data = educationModel;
            }
            else
            {
                jsonReturnModel.Code = 404;
                jsonReturnModel.MessageTitle = "Bulunamadı";
                jsonReturnModel.Message = "Eğitim yok";

                return Error(jsonReturnModel);
            }

            return Succes(jsonReturnModel);
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        [Route("postedit")]
        public async Task<IActionResult> PostEdit(EducationModel Model)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Model.EducationId && !x.IsRemoved);
            var AuthenticatedUserId = User.Identity.GetUserId();
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (Education != null)
            {
                if (Education.UserId == long.Parse(AuthenticatedUserId))
                {
                    if (Education.IsUniversityInformationsCanEditable())
                    {
                        Education.UniversityId = Model.UniversityId;
                        Education.Department = Model.Department;
                    }
                    if (Model.Startyear <= Model.Finishyear)
                    {
                        Education.StartYear = Model.Startyear.ToString();
                        Education.EndYear = Model.Finishyear.ToString();
                    }

                    Education.ActivitiesSocieties = Model.ActivitiesSocieties;
                    try
                    {
                        var result = await Context.SaveChangesAsync();
                        if (result > 0)
                        {
                            if (!(Model.Startyear <= Model.Finishyear))
                            {
                                jsonReturnModel.Message = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı" +
                                    "<br />" + "Bunlar dışında ki eğitim bilgileriniz düzenlendi";
                                return Succes(jsonReturnModel);
                            }
                            else
                            {
                                jsonReturnModel.Message = "Eğitim bilgileriniz düzenlendi";
                                return Succes(jsonReturnModel);
                            }
                        }
                        else
                        {
                            if (!(Model.Startyear <= Model.Finishyear))
                            {
                                jsonReturnModel.Message = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı";
                            }
                            else
                            {
                                jsonReturnModel.Message = "Bir değişiklik yapılmadı";
                            }
                            jsonReturnModel.Code = 1001;


                            return Error(jsonReturnModel);
                        }
                    }
                    catch (System.Exception e)
                    {
                        if (e.InnerException.Message.ToLower().Contains("department"))
                        {
                            jsonReturnModel.Code = 200;
                            jsonReturnModel.Message = "Eğitimin bölümüyle alakalı bir şeylere ulaşamadık";
                        }
                        else if (e.InnerException.Message.ToLower().Contains("university"))
                        {
                            jsonReturnModel.Code = 200;
                            jsonReturnModel.Message = "Eğitimin üniversitesi alakalı bir şeylere ulaşamadık";
                        }

                        return Error(jsonReturnModel);
                    }

                }
                else
                {
                    jsonReturnModel.Code = 200;
                    jsonReturnModel.Message = "McHammer: You can't touch this";
                    return Error(jsonReturnModel);
                }
            }

            return Error(jsonReturnModel);
        }
    }
}
