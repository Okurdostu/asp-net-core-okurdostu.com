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
            rm.Succes = true;

            if (rm.Code == 201)
            {
                return CreatedAtAction(null, rm);
            }

            rm.Code = 200;
            return Ok(rm);
        }

        public ActionResult Error(ReturnModel rm)
        {
            rm.Succes = false;

            if (rm.Code == 500)
            {
                return StatusCode(500, rm);
            }
            if (rm.Code == 404)
            {
                return NotFound(rm);
            }

            if (rm.Code == 1001)
            {
                rm.Message = "Hiç bir değişiklik yapılmadı";
                rm.InternalMessage = "Changes aren't save";
                return BadRequest(rm);
            }

            rm.Code = 400;
            return BadRequest(rm);
        }
    }
}
