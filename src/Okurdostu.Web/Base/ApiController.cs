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
                jsonReturnModel.Message = "Authorized user not found";
                jsonReturnModel.MessageTitle = "Unauthorized";
                return Unauthorized(jsonReturnModel);
            }
            else if (jsonReturnModel.Code == 403)
            {
                return Forbid();
            }
            else if (jsonReturnModel.Code == 404)
            {
                return (NotFound(jsonReturnModel));
            } //10001: db'de değişiklik yapmaya çalışılırken hiç bir verinin değiştirilmediğini durumu: db.savechanges resultının 0 gelmesi.
            else if (jsonReturnModel.Code == 1001 || jsonReturnModel.Code == 200)
            {
                
                return (Ok(jsonReturnModel));
            }

            jsonReturnModel.Code = 400;
            return BadRequest(jsonReturnModel);
        }
    }
}
