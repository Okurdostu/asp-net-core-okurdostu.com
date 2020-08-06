using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;
using Okurdostu.Web.Models;

namespace Okurdostu.Web.Base
{
    [Route("api/[controller]")]
    public class ApiController : BaseController<ApiController>
    {
        public User AuthenticatedUser;

        public ActionResult Succes(ReturnModel rm)
        {
            rm.Code = 200;
            rm.Succes = true;

            return Ok(rm);
        }
        public ActionResult Error(ReturnModel rm)
        {
            rm.Succes = false;

            if (rm.Code == 500)
            {
                return StatusCode(500, rm);
            }
            if (rm.Code == 401)
            {
                return Unauthorized();
            }
            if (rm.Code == 403)
            {
                return Forbid();
            }
            if (rm.Code == 404)
            {
                return NotFound(rm);
            }


            rm.Code = 400;
            return BadRequest(rm);
        }
    }
}
