using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    public class CommentsController : SecureApiController
    {
        [HttpGet("")]
        public ActionResult Index()
        {
            return NotFound();
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetSingle(Guid Id)
        {
            ReturnModel rm = new ReturnModel();
            //when user does reply a comment or wants to edit their comment, it works to view comment that will be edited or replied.
            var RequestedComment = await Context.NeedComment.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (RequestedComment != null)
            {
                rm.Data = new { comment = RequestedComment.Comment, username = RequestedComment.User.Username, fullname = RequestedComment.User.FullName };
                return Succes(rm);
            }
            else
            {
                rm.Code = 200;
                rm.Message = "Böyle bir yorum yok";
                return Error(rm);
            }
        }

        public class CommentModel
        {
            public Guid? NeedId { get; set; }
            public Guid? RelatedCommentId { get; set; }

            [Required(ErrorMessage = "Bir şeyler yazmalısın")]
            [MaxLength(100, ErrorMessage = "En fazla 100 karakter")]
            [DataType(DataType.MultilineText)]
            public string Comment { get; set; }
        }
        [ServiceFilter(typeof(ConfirmedEmailFilter))]
        [HttpPost("")]
        public async Task<IActionResult> PostAdd(CommentModel model) //doing comment or reply
        {
            ReturnModel rm = new ReturnModel();

            if (model.NeedId == null && model.RelatedCommentId == null || model.NeedId == Guid.Empty && model.RelatedCommentId == null)
            {
                rm.Message = "Yorum yapmak istediğiniz kampanyayı veya cevaplamak istedğiniz yorumu kontrol edin";
                return Error(rm);
            }

            if (!ModelState.IsValid)
            {
                if (model.Comment == null)
                {
                    rm.Message = "Bir şeyler yazmalısın";
                }
                else if (model.Comment.Length > 100)
                {
                    rm.Message = "En fazla 100 karakter";
                }
                rm.InternalMessage = "Comment is required";

                return Error(rm);
            }

            if (model.NeedId != Guid.Empty && model.NeedId != null) // add new comment
            {
                var commentedNeed = await Context.Need.AnyAsync(x => x.Id == model.NeedId && !x.IsRemoved && x.IsConfirmed);

                if (commentedNeed)
                {
                    var NewComment = new NeedComment
                    {
                        Comment = model.Comment,
                        NeedId = (Guid)model.NeedId,
                        UserId = Guid.Parse(User.Identity.GetUserId())
                    };

                    await Context.AddAsync(NewComment);
                    var result = await Context.SaveChangesAsync();

                    if (result > 0)
                    {
                        rm.Data = NewComment.Id;
                        return Succes(rm);
                    }
                    else
                    {
                        rm.Message = "Yorumunuz kaydolmadı";
                        return Error(rm);
                    }
                }
                else
                {
                    rm.Message = "Tartışmanın başlatılacağı kampanya yok veya burada tartışma başlatılamaz";
                    return Error(rm);
                }
            }
            else  //[reply] add relational comment
            {
                var repliedComment = await Context.NeedComment.Include(comment => comment.Need).FirstOrDefaultAsync(x => x.Id == model.RelatedCommentId && !x.IsRemoved && !x.Need.IsRemoved && x.Need.IsConfirmed);

                if (repliedComment != null)
                {
                    var NewReply = new NeedComment
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
                        rm.Data = NewReply.Id;
                        rm.Message = "Cevapladınız";
                        return Succes(rm);
                    }
                    else
                    {
                        rm.Code = 200;
                        rm.Message = "Başaramadık, ne olduğunu bilmiyoruz";
                        return Error(rm);
                    }
                }
                else
                {
                    rm.Code = 200;
                    rm.Message = "Cevaplanacak yorum yok, silinmiş veya burada cevap verilemez";
                    return Error(rm);
                }
            }
        }
        [HttpPatch("remove/{Id}")]
        public async Task<IActionResult> PatchRemove(Guid Id)
        {
            ReturnModel rm = new ReturnModel();
            if (Id == null || Id == Guid.Empty)
            {
                rm.Message = "Silmek için yorum seçmediniz";
                rm.InternalMessage = "Id is required";
                return Error(rm);
            }

            var DeletedComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && x.UserId == Guid.Parse(User.Identity.GetUserId()));
            if (DeletedComment != null)
            {
                DeletedComment.IsRemoved = true;
                DeletedComment.UserId = null; // hangi user'ın bu veriyi girdiği boş bırakılmalı
                DeletedComment.Comment = ""; // aynı şekilde içerik yok edilmeli
                await Context.SaveChangesAsync();

                rm.Message = "Yorumunuz silindi";
                return Succes(rm);
            }
            else
            {
                rm.Message = "Silmeye çalıştığınız yorum yok";
                return Error(rm);
            }
        }
        public class EditCommentModel
        {
            [Required]
            public Guid Id { get; set; }

            [Required(ErrorMessage = "Bir şeyler yazmalısın")]
            [MaxLength(100, ErrorMessage = "En fazla 100 karakter")]
            [DataType(DataType.MultilineText)]
            public string Comment { get; set; }
        }
        [HttpPatch("{Id}")]
        public async Task<IActionResult> PatchEdit(EditCommentModel model)
        {
            ReturnModel rm = new ReturnModel();
            if (!ModelState.IsValid)
            {
                if (model.Comment == null)
                {
                    rm.Message = "Bir şeyler yazmalısın";
                }
                else if (model.Comment.Length > 100)
                {
                    rm.Message = "En fazla 100 karakter";
                }

                rm.InternalMessage = "Id and comment are required";

                return Error(rm);
            }
            var EditedComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsRemoved && x.UserId == Guid.Parse(User.Identity.GetUserId()));
            if (EditedComment != null)
            {
                if (EditedComment.Comment != model.Comment)
                {
                    EditedComment.Comment = model.Comment;
                    await Context.SaveChangesAsync();
                    rm.Message = "Yorum içeriğiniz düzenlendi";
                    return Succes(rm);
                }
                else
                {
                    rm.Message = "Aynı içerik ile düzenlemeye çalıştınız";
                    return Error(rm);
                }
            }
            else
            {
                rm.Message = "Düzenlemeye çalıştığınız yorum yok";
                return Error(rm);
            }
        }
    }
}