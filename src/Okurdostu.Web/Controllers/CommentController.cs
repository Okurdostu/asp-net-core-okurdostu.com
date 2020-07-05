using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class CommentController : BaseController<CommentController>
    {
        public User AuthUser;

        [Authorize]
        [Route("~/Comment")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Comment(CommentModel Model) //main comment
        {
            //  flood ihtimalleri:
            //  aynı veya benzer comment içeriğini girme,
            //  çok fazla ard arda giriş yapma.

            if (Model.NeedId != null)
            {
                return null;
            }

            var CommentedNeed = await Context.Need.FirstOrDefaultAsync(x => x.Id == Model.NeedId && x.IsConfirmed && !x.IsRemoved);

            if (CommentedNeed != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

                var NewComment = new NeedComment()
                {
                    Comment = Model.Comment,
                    UserId = AuthUser.Id,
                    NeedId = (long)Model.NeedId
                };

                await Context.AddAsync(NewComment);
                var result = await Context.SaveChangesAsync();

                if (result > 0)
                {
                    return Json(new { id = NewComment.Id });
                }
                else
                {
                    return Json(new { error = "Başaramadık, ne olduğunu bilmiyoruz" });
                }
            }
            else
            {
                return Json(new { error = "Böyle bir şey yok" });
            }

        }


        [Authorize]
        [Route("~/DeleteComment")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteComment(Guid id)
        {
            var DeletedComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == id && !x.IsRemoved);

            if (DeletedComment != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

                if (AuthUser.Id == DeletedComment.User.Id)
                {
                    DeletedComment.IsRemoved = true;
                    var result = await Context.SaveChangesAsync();
                    return Json(true);
                }
            }

            return null;
        }


        [Route("/Comments")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Comments(long id)
        {
            var result = await Context.NeedComment.Where(x => x.NeedId == id).Include(x => x.User).OrderBy(x => x.CreatedOn).ToListAsync();
            return View(result);
        }
    }
}
