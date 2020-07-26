using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "LikeCount")]
    public class LikeCountViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public LikeCountViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync(Guid id)
        {
            int Count = await Context.NeedLike.Where(x => x.IsCurrentLiked && x.NeedId == id).Select(a => a.User).CountAsync();
            return View(Count);
        }
    }
}
