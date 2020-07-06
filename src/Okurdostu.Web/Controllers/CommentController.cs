using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
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

            if (Model.NeedId == null)
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
        public async Task<JsonResult> DeleteComment(Guid Id)
        {
            var DeletedComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (DeletedComment != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

                if (AuthUser.Id == DeletedComment.User.Id)
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
        [Route("~/GetCommentContent")]
        public async Task<JsonResult> GetCommentContent(Guid Id) //it's for edit a comment
        {
            var EditingComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (EditingComment != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

                if (AuthUser.Id == EditingComment.User.Id)
                {
                    return Json(EditingComment.Comment);
                }
            }

            return Json(false);
        }

        [Authorize]
        [Route("~/EditComment")]
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

                    if (AuthUser.Id == EditedComment.User.Id)
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
                            if (EditedComment.Comment.Length > 100)
                            {
                                state = false;
                                errorMessage = "En fazla 100 karakter";
                                infoMessage = "";

                            }
                            else
                            {
                                state = false;
                                infoMessage = "Ne olduğunu bilmiyoruz tekrar deneyin.";
                                errorMessage = "";
                            }
                        }
                    }
                }
                else
                {
                    state = false;
                    errorMessage = "Aynı içeriği giriyorsunuz.";
                    infoMessage = "";
                }
            }

            return Json(new { state, infoMessage, errorMessage });
        }

        [Route("/Comments")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Comments(long Id)
        {
            var result = await Context.NeedComment.Where(x => x.NeedId == Id).Include(x => x.User).OrderBy(x => x.CreatedOn).ToListAsync();
            return View(result);
        }
    }
}
