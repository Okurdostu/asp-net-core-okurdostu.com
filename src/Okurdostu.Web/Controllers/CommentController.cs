using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace Okurdostu.Web.Controllers
{
    public class CommentController : BaseController<CommentController>
    {
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Comments(Guid Id)
        {
            var result = await Context.NeedComment.Where(x => x.NeedId == Id).Include(x => x.User).OrderBy(x => x.CreatedOn).ToListAsync();
            return View(result);
        }
    }
}
