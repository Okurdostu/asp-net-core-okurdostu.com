using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Okurdostu.Web.Filters;
namespace Okurdostu.Web.Controllers
{
    public class CommentController : BaseController<CommentController>
    {
        public User AuthUser;

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        public async Task<JsonResult> Comment(CommentModel Model) //main comment & reply a comment
        {
            //  flood ihtimalleri:
            //  aynı veya benzer comment içeriğini girme,
            //  çok fazla ard arda giriş yapma.

            if (!ModelState.IsValid)
            {
                return Json(null);
            }

            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (Model.NeedId != null)
            {
                var CommentedNeed = await Context.Need.FirstOrDefaultAsync(x => x.Id == Model.NeedId && !x.IsRemoved && x.IsConfirmed);
                if (CommentedNeed != null)
                {
                    var NewComment = new NeedComment()
                    {
                        Comment = Model.Comment,
                        UserId = AuthUser.Id,
                        NeedId = (long)Model.NeedId
                    };
                    await Context.AddAsync(NewComment);
                    await Context.SaveChangesAsync();
                    return Json(new { id = NewComment.Id }); //returning comment id for sliding page to the new comment
                }
                else
                {
                    return Json(new { infoMessage = "Tartışma başlatılacak kampanyaya ulaşamadık" });
                }
            }
            else if (Model.RelatedCommentId != null)
            {
                var RepliedComment = await Context.NeedComment.Include(x => x.Need).
                FirstOrDefaultAsync(x => x.Id == Model.RelatedCommentId && !x.IsRemoved && !x.Need.IsRemoved && x.Need.IsConfirmed);
                if (RepliedComment != null)
                {
                    var NewReply = new NeedComment()
                    {
                        Comment = Model.Comment,
                        UserId = AuthUser.Id,
                        NeedId = RepliedComment.NeedId,
                        RelatedCommentId = RepliedComment.Id 
                    };

                    await Context.AddAsync(NewReply);
                    await Context.SaveChangesAsync();
                    return Json(new { id = NewReply.Id }); //returning comment id for sliding page to the new comment
                }
                else
                {
                    return Json(new { infoMessage = "Cevaplanacak yoruma ulaşamadık" });
                }
            }

            return Json(null);
        }


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteComment(Guid Id)
        {
            var DeletedComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (DeletedComment != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

                if (AuthUser.Id == DeletedComment.UserId)
                {
                    DeletedComment.IsRemoved = true;
                    DeletedComment.UserId = null;
                    DeletedComment.Comment = "";
                    await Context.SaveChangesAsync();
                    return Json(true);
                }
            }

            return Json(false);
        }


        [HttpGet]
        public async Task<JsonResult> GetContent(Guid Id) //it's for edit and doreply
        {
            //when user does reply a comment or wants to edit their comment, it works to view comment that will be edited or replied.
            var RequestedComment = await Context.NeedComment.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (RequestedComment != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                return Json(new { state = true, comment = RequestedComment.Comment, username = RequestedComment.User.Username, fullname = RequestedComment.User.FullName });
            }

            return Json(new { state = false});
        }


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> EditComment(Guid Id, string EditCommentInput)
        {
            bool state = false;
            string errorMessage = "";
            string infoMessage = "";

            if (EditCommentInput == null || EditCommentInput.Length == 0 || EditCommentInput.Length > 100)
            {
                return Json(new { state, errorMessage = "En fazla 100 karakter ve boş olamaz" });
            }

            var EditedComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (EditedComment != null)
            {
                if (EditedComment.Comment != EditCommentInput)
                {
                    AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

                    if (AuthUser.Id == EditedComment.UserId)
                    {
                        try
                        {
                            EditedComment.Comment = EditCommentInput;
                            await Context.SaveChangesAsync();
                            state = true;
                        }
                        catch (Exception)
                        {
                            state = false;

                            if (EditedComment.Comment.Length > 100)
                            {
                                errorMessage = "En fazla 100 karakter";
                                infoMessage = "";
                            }
                            else
                            {
                                infoMessage = "Ne olduğunu bilmiyoruz tekrar deneyin.";
                                errorMessage = "";
                            }
                        }
                    }
                }
            }

            return Json(new { state, infoMessage, errorMessage });
        } 


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Comments(long Id)
        {
            var result = await Context.NeedComment.Where(x => x.NeedId == Id).Include(x => x.User).OrderBy(x => x.CreatedOn).ToListAsync();
            return View(result);
        }
    }
}
