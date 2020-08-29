using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class LikeController : BaseController<LikeController>
    {
        [HttpPost, Authorize]
        [Route("Like")]
        public async Task<JsonResult> Index(Guid id, string username)
        {
            var Need = await Context.Need.FirstOrDefaultAsync(x =>
                x.Id == id && x.User.Username == username && !x.IsRemoved && x.IsConfirmed
            );
            if (Need != null)
            {
                var AuthUserId = Guid.Parse(User.Identity.GetUserId());
                var PreviouslyLike = await Context.NeedLike.FirstOrDefaultAsync(x => x.UserId == AuthUserId && x.NeedId == Need.Id);

                if (PreviouslyLike != null)
                {
                    PreviouslyLike.IsCurrentLiked = !PreviouslyLike.IsCurrentLiked;
                }
                else
                {
                    var NewLike = new NeedLike
                    {
                        NeedId = Need.Id,
                        UserId = AuthUserId
                    };
                    await Context.AddAsync(NewLike);

                }

                await Context.SaveChangesAsync();
                return Json(new { succes = true });
            }
            return Json(new { error = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("Like/Likers")]
        public async Task<ActionResult> LikersAsync(Guid id, string username) => View(await Context.NeedLike.Where(x => x.IsCurrentLiked && x.NeedId == id && x.Need.User.Username == username).Select(a => a.User).ToListAsync());

        [Route("Like/Count/{id}")]
        public async Task<int> CountAsync(Guid id) => await Context.NeedLike.CountAsync(x => x.IsCurrentLiked && x.NeedId == id);

        [Authorize]
        [Route("Like/LikeState/{id}")]
        public async Task<bool> LikeStateAsync(Guid id) => await Context.NeedLike.AnyAsync(x => x.IsCurrentLiked && x.NeedId == id && x.UserId == Guid.Parse(User.Identity.GetUserId()));
    }
}
