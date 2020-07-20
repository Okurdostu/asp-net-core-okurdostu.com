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
            if (User.Identity.IsAuthenticated)
            {
                TempData["emailstate"] = User.Identity.GetEmailConfirmStatus();
                TempData["email"] = User.Identity.GetEmail();
            }

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(FeedbackModel Model)
        {
            if (ModelState.IsValid)
            {
                var Feedback = new Feedback
                {
                    Email = Model.Email,
                    Message = Model.Message
                };
                await Context.AddAsync(Feedback);
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                    TempData["BetaMessage"] = "Geri bildiriminiz iletildi, teşekkür ederiz";
            }
            else
                TempData["BetaMessage"] = "Gerekli bilgileri doldurmadınız.";

            return Redirect("/beta");
        }
    }
}
