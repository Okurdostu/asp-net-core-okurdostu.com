using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.Base
{
    [Route("api/[controller]")]
    public class ApiController : BaseController<ApiController>
    {
        public User AuthenticatedUser;

        [NonAction]
        public async Task<bool> ConfirmIdentityWithPassword(string ConfirmPassword)
        {
            AuthenticatedUser = await GetAuthenticatedUserFromDatabaseAsync();
            ConfirmPassword = ConfirmPassword.SHA512();
            return ConfirmPassword == AuthenticatedUser.Password;
        }

        public ActionResult Succes(JsonReturnModel jsonReturnModel)
        {
            jsonReturnModel.Code = 200;
            jsonReturnModel.Succes = true;

            return Ok(jsonReturnModel);
        }
        public ActionResult Error(JsonReturnModel jsonReturnModel)
        {
            jsonReturnModel.Succes = false;

            if (jsonReturnModel.Code == 500)
            {
                return StatusCode(500, jsonReturnModel);
            }
            if (jsonReturnModel.Code == 401)
            {
                return Unauthorized();
            }
            if (jsonReturnModel.Code == 403)
            {
                return Forbid();
            }
            if (jsonReturnModel.Code == 404)
            {
                return NotFound();
            }
            

            jsonReturnModel.Code = 400;
            return BadRequest(jsonReturnModel);
        }
    }
}
