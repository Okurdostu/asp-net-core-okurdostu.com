using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class CommentController : BaseController<CommentController>
    {
        [Route("/Comments")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Comments(long id)
        {
            var result = await Context.NeedComment.Where(x => x.NeedId == id).Include(x => x.User).OrderByDescending(x => x.InverseRelatedComment.Count()).ToListAsync();
            return View(result);
        }
    }
}
