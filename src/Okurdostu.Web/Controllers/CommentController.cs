using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Okurdostu.Web.Controllers
{
    public class CommentController : BaseController<CommentController>
    {
        [Route("~/Comment/Post")]
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> DoComment(long? NeedId, Guid? RelatedId, string _Comment)
        {
            long AuthUserId = long.Parse(User.Identity.GetUserId());

            if (!await Context.Need.AnyAsync(x => x.Id == NeedId))
            {
                return Json(new { error = "there isn't need data", succes = false });
            }

            var needComment = new NeedComment()
            {
                NeedId = (long)NeedId,
                UserId = AuthUserId,
                Comment = _Comment
            };

            if (RelatedId != null)
            {
                var RelateComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == RelatedId);

                if (RelateComment != null)
                {
                    //ilişki kurmaya çalıştığı yorum: başka bir yoruma bağlandıysa o bağlandığı yorumun mainin kendi mainine ata:
                    //eğer ki ilişki kurmaya çalıştığı yorum başka hiç bir yoruma bağlanmadıysa, ilişki kurduğu yorumu kendi mainine ata.
                    needComment.RelatedMainCommentId = RelateComment.RelatedMainCommentId != null ? RelateComment.RelatedMainCommentId : RelateComment.Id;
                    //örnek: 1, 2, 3 ve 4 idli veya numaralı (her ne derseniz) yorumlar var
                    //1. yorum tek başına hiç bir yorumla ilişki kurmadan oluşturulmuş bir yorum:   maincommentid: null relatedcommentid: null

                    //2. yorum 1. yorum ile bağlantı kurmuş bir yorum                               maincommentid: 1    relatedcommentid: 1         (if bloğunda işlem yaptı)
                    //3. yorum 1. yorum ile bağlantı kurmuş bir yorum                               maincommentid: 1    relatedcommentid: 1         (if bloğunda işlem yaptı)
                    //4. yorum 2. yorum ile bağlantı kurmuş bir yorum                               maincommentid: 1    relatedcommentid: 2         (else bloğunda işlem yaptı)

                    needComment.RelatedCommentId = RelateComment.Id;
                    needComment.NeedId = RelateComment.NeedId;
                }
                else
                {
                    return Json(new { error = "there isn't comment to relate", succes = false });
                }
            }

            await Context.AddAsync(needComment);
            await Context.SaveChangesAsync();
            return Json(new { succes = true });
        }

        [Route("/Comments")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GetComments(long id) => View(
            await Context.NeedComment.Include(a => a.User)
            .Where(x => x.NeedId == id && !x.IsRemoved)
            .OrderBy(x => x.CreatedOn).ToListAsync());
    }
}
