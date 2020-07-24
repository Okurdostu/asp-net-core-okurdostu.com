using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;

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
    }
}
