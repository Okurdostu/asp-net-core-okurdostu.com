using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "LikeButton")]
    public class LikeButtonViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public LikeButtonViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync(long id)
        {
            var AuthUser = await Context.User.FirstOrDefaultAsync(x => x.Id.ToString() == User.Identity.GetUserId());
            bool isLiked = false;
            if (Context.NeedLike.Where(x => x.IsCurrentLiked == true && x.NeedId == id && x.UserId == AuthUser.Id).FirstOrDefault() != null)
                isLiked = true;

            return View(isLiked);
        }
    }
}
