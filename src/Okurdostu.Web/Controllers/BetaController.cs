using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class BetaController : BaseController<BetaController>
    {
        [Route("beta")]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated && User.Identity.GetEmailConfirmStatus() != true)
            {
                ViewData["emailconfirmstatus"] = User.Identity.GetEmailConfirmStatus();
                ViewData["email"] = User.Identity.GetEmail();
            }

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(FeedbackModel Model)
        {
            ReturnModel rm = new ReturnModel();
            if (ModelState.IsValid)
            {
                var Feedback = new Feedback
                {
                    Email = Model.Email,
                    Message = Model.Message
                };
                await Context.AddAsync(Feedback).ConfigureAwait(false);
                await Context.SaveChangesAsync().ConfigureAwait(false);

                rm.Code = 200;
                rm.Succes = true;
                rm.Message = "Geri bildiriminiz iletildi, teşekkür ederiz";
                return Ok(rm);
            }
            else
            {
                rm.Code = 400;
                rm.Succes = false;
                rm.InternalMessage = "Model is invalid";
                return BadRequest(rm);
            }
        }
    }
}
