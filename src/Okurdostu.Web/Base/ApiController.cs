using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.Base
{
    [Route("api/[controller]")]
    public class ApiController : BaseController<ApiController>
    {
        public ActionResult Succes(JsonReturnModel jsonReturnModel)
        {
            jsonReturnModel.Code = 200;
            jsonReturnModel.Status = true;

            return Ok(jsonReturnModel);
        }
        public ActionResult Error(JsonReturnModel jsonReturnModel)
        {
            jsonReturnModel.Status = false;

            if (jsonReturnModel.Code == 401)
            {
                jsonReturnModel.MessageTitle = "Unauthorized";
                jsonReturnModel.MessageTitle = "Authorized user not found"; //client'da bunu yakala ve girişe yönlendir
                return Unauthorized(jsonReturnModel);
            }
            else if (jsonReturnModel.Code == 403)
            {
                return Forbid();
            }
            else if (jsonReturnModel.Code == 404)
            {
                return (NotFound(jsonReturnModel));
            }

            jsonReturnModel.Code = 400;
            return BadRequest(jsonReturnModel);
        }
    }
}
