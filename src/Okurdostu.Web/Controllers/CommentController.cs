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

            if (Model.Comment != null)
            {
                if (Model.Comment.Length > 100)
                {
                    return Json(new { errorMessage = "En fazla 100 karakter" });
                }
                else if (Model.Comment.Length < 5)
                {
                    return Json(new { errorMessage = "En az 5 karakter" });
                }
            }
            else
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
                    return Json(new { id = NewComment.Id });
                }
                else
                {
                    return Json(new { infoMessage = "Bir şeylere ulaşamadık" });
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
                    return Json(new { id = NewReply.Id });
                }
                else
                {
                    return Json(new { infoMessage = "Bir şeylere ulaşamadık" });
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
                    //bir daha görüntülenemeyecek yorum(geri getirme olmayacak): userini veya comment içeriğini tutmaya gerek yok
                    //ve hiyerarşik ağaç yapısını bozmaması için tamamen kaldırmıyorum
                    await Context.SaveChangesAsync();
                    return Json(true);
                }
            }

            return Json(false);
        }


        [HttpGet]
        public async Task<JsonResult> GetContent(Guid Id) //it's for edit and doreply
        {
            var Comment = await Context.NeedComment.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (Comment != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                return Json(new { state = true, comment = Comment.Comment, username = Comment.User.Username, fullname = Comment.User.FullName });
            }

            return Json(new { state = false, comment = "" });
        }


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> EditComment(Guid Id, string EditCommentInput)
        {
            bool state = false;
            string errorMessage = "";
            string infoMessage = "";

            if (EditCommentInput == null || EditCommentInput.Length < 5)
            {
                return Json(new { state, errorMessage = "En az 5 karakter" });
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
                            errorMessage = "";
                            infoMessage = "";
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
                else
                {
                    errorMessage = "Aynı içeriği giriyorsunuz.";
                    infoMessage = "";
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
