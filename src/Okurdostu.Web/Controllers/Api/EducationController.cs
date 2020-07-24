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
        public async Task<JsonResult> Get(long EducationId)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == EducationId);

            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (Education != null)
            {
                var AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

                if (AuthUser == null || AuthUser.Id != Education.UserId)
                {
                    jsonReturnModel.Status = false;
                    jsonReturnModel.MessageTitle = "MC Hammer";
                    jsonReturnModel.Message = "You can't touch this";
                    return Json(jsonReturnModel);
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

                jsonReturnModel.Status = true;
                jsonReturnModel.Data = educationModel;
            }
            else
            {
                jsonReturnModel.Status = false;
                jsonReturnModel.MessageTitle = "Başarısız";
                jsonReturnModel.Message = "Eğitime ulaşamadık";
            }

            return Json(jsonReturnModel);
        }
    }
}
