using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class LikeController : BaseController<LikeController>
    {
        private User AuthUser = null;

        [Route("~/Like")]
        [HttpPost, Authorize]
        public async Task<IActionResult> Index(int id, string username)
        {
            var Need = await Context.Need.FirstOrDefaultAsync(x =>
                x.Id == id && x.User.Username == username && !x.IsRemoved && x.IsConfirmed
            );
            if (Need != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

                var PreviouslyLike = await Context.NeedLike.FirstOrDefaultAsync(x => x.UserId == AuthUser.Id && x.NeedId == Need.Id);

                if (PreviouslyLike != null)
                {
                    PreviouslyLike.IsCurrentLiked = !PreviouslyLike.IsCurrentLiked;
                }
                else
                {

                    var NewLike = new NeedLike
                    {
                        NeedId = Need.Id,
                        UserId = AuthUser.Id,
                        IsCurrentLiked = true,
                    };
                    await Context.AddAsync(NewLike);

                }

                await Context.SaveChangesAsync();
                return Json(new { succes = true });
            }
            return Json(new { error = true });
        }

        [Route("~/Like/Likers/{id}/{username}")]
        public ActionResult Likers(int id, string username)
        {
            List<User> Likers = Context.NeedLike.Where(x =>
               x.IsCurrentLiked
            && x.NeedId == id
            && x.Need.User.Username == username).Select(a => a.User).ToList();
            return View(Likers);
        }

        [Route("Like/Count/{id}")]
        public ActionResult Count(int id) //get likers count
        {
            int Count = Context.NeedLike.Where(x => x.IsCurrentLiked && x.NeedId == id).Select(a => a.User).ToList().Count();
            return View(Count);
        }

        [Authorize]
        [Route("Like/Button/{id}")]
        public async Task<IActionResult> Button(int id)
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            bool isLiked = false;
            if (Context.NeedLike.Where(x => x.IsCurrentLiked == true && x.NeedId == id && x.UserId == AuthUser.Id).FirstOrDefault() != null)
                isLiked = true;

            return View(isLiked);
        }
    }
}
