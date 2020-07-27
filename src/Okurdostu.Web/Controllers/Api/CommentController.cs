using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MimeKit;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    public class CommentController : ApiController
    {
        public class CommentModel
        {
            public Guid? NeedId { get; set; }
            public Guid? RelatedCommentId { get; set; }

            [Required(ErrorMessage = "Bir şeyler yazmalısın")]
            [MaxLength(100, ErrorMessage = "En fazla 100 karakter")]
            [DataType(DataType.MultilineText)]
            public string Comment { get; set; }
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get(Guid Id) //get single comment for edit and doing reply
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();
            //when user does reply a comment or wants to edit their comment, it works to view comment that will be edited or replied.
            var RequestedComment = await Context.NeedComment.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (RequestedComment != null)
            {
                jsonReturnModel.Data = new { comment = RequestedComment.Comment, username = RequestedComment.User.Username, fullname = RequestedComment.User.FullName };
                return Succes(jsonReturnModel);
            }
            else
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "Böyle bir yorum yok";
                return Error(jsonReturnModel);
            }
        }

        [Authorize]
        [HttpPost("do")]
        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        public async Task<IActionResult> Do(CommentModel model) //doing comment or reply
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();
            if (!ModelState.IsValid || model.NeedId != null && model.RelatedCommentId != null)
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "Bu şekilde bir giriş yapamazsınız";
                return Error(jsonReturnModel);
            }

            if (model.NeedId != null) // add new comment
            {
                var commentedNeed = await Context.Need.AnyAsync(x => x.Id == model.NeedId && !x.IsRemoved && x.IsConfirmed);

                if (commentedNeed)
                {
                    var NewComment = new NeedComment()
                    {
                        Comment = model.Comment,
                        NeedId = (Guid)model.NeedId,
                        UserId = Guid.Parse(User.Identity.GetUserId())
                    };

                    await Context.AddAsync(NewComment);
                    var result = await Context.SaveChangesAsync();

                    if (result > 0)
                    {
                        jsonReturnModel.Data = NewComment.Id;
                        jsonReturnModel.Message = "Yorumunuz eklendi";
                        return Succes(jsonReturnModel);
                    }
                    else
                    {
                        jsonReturnModel.Code = 200;
                        jsonReturnModel.Message = "Başaramadık, ne olduğunu bilmiyoruz";
                        return Error(jsonReturnModel);
                    }
                }
                else
                {
                    jsonReturnModel.Code = 200;
                    jsonReturnModel.Message = "Tartışmanın başlatılacağı kampanya yok veya burada tartışma başlatılamaz";
                    return Error(jsonReturnModel);
                }
            }
            else if (model.RelatedCommentId != null) //[reply] add relational comment
            {
                var repliedComment = await Context.NeedComment.Include(comment => comment.Need).FirstOrDefaultAsync(x => x.Id == model.RelatedCommentId && !x.IsRemoved && !x.Need.IsRemoved && x.Need.IsConfirmed);

                if (repliedComment != null)
                {
                    var NewReply = new NeedComment()
                    {
                        Comment = model.Comment,
                        UserId = Guid.Parse(User.Identity.GetUserId()),
                        NeedId = repliedComment.NeedId,
                        RelatedCommentId = repliedComment.Id
                    };

                    await Context.AddAsync(NewReply);
                    var result = Context.SaveChanges();

                    if (result > 0)
                    {
                        jsonReturnModel.Data = NewReply.Id;
                        jsonReturnModel.Message = "Cevapladınız";
                        return Succes(jsonReturnModel);
                    }
                    else
                    {
                        jsonReturnModel.Code = 200;
                        jsonReturnModel.Message = "Başaramadık, ne olduğunu bilmiyoruz";
                        return Error(jsonReturnModel);
                    }
                }
                else
                {
                    jsonReturnModel.Code = 200;
                    jsonReturnModel.Message = "Cevaplanacak yorum yok, silinmiş veya burada cevap verilemez";
                    return Error(jsonReturnModel);
                }
            }
            else //bad request
            {
                jsonReturnModel.Message = "Tartışma başlatma veya yorum yapma seçilemedi";
                return Error(jsonReturnModel);
            }
        }

        [Authorize]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();
            var DeletedComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && x.UserId == Guid.Parse(User.Identity.GetUserId()));

            if (DeletedComment != null)
            {
                DeletedComment.IsRemoved = true;
                DeletedComment.UserId = null; // hangi user'ın bu veriyi girdiği boş bırakılmalı
                DeletedComment.Comment = ""; // aynı şekilde içerik yok edilmeli
                await Context.SaveChangesAsync();

                jsonReturnModel.Message = "Başarılı, yorumunuz silindi";
                return Succes(jsonReturnModel);
            }
            else
            {
                jsonReturnModel.Code = 200;
                jsonReturnModel.Message = "Silmeye çalıştığınız yorum yok";
                return Error(jsonReturnModel);
            }
        }
    }
}
