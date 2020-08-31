using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;
using Okurdostu.Web.Models;

namespace Okurdostu.Web.Base
{
    [Route("api/[controller]")]
    public class ApiController : BaseController<ApiController>
    {
        public User AuthenticatedUser;

        public ActionResult Succes(string message = null, object data = null, int code = 200)
        {
            var rm = new ReturnModel
            {
                Code = code,
                Data = data,
                InternalMessage = null,
                Message = message,
                Succes = true
            };
            if (rm.Code == 201)
            {
                return CreatedAtAction(null, rm);
            }

            return Ok(rm);
        }

        public ActionResult Error(string message = null, string internalMessage = null, object data = null, int code = 400)
        {
            var rm = new ReturnModel
            {
                Code = code,
                Data = data,
                InternalMessage = internalMessage,
                Message = message,
                Succes = false
            };

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

            return BadRequest(rm);
        }
    }
}
