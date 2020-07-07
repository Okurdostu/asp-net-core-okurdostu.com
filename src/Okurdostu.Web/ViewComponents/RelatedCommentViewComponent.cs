using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "RelatedComment")]
    public class RelatedCommentViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public RelatedCommentViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync(Guid id)
        {
            return View(await Context.NeedComment.Where(x => x.RelatedCommentId == id).OrderByDescending(x => x.InverseRelatedComment.Count()).ToListAsync());
        }

    }
}
