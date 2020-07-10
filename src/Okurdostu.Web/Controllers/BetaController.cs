using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class BetaController : BaseController<BetaController>
    {
        [Route("beta")]
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (!AuthUser.IsEmailConfirmed) //bu bilgileri claimslerden al.
                {
                    TempData["emailstate"] = AuthUser.IsEmailConfirmed;
                    TempData["email"] = AuthUser.Email;
                }
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
