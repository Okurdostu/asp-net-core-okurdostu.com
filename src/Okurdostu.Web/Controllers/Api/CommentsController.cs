using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
            //when user does reply a comment or wants to edit their comment, it works to view comment that will be edited or replied.
            var RequestedComment = await Context.NeedComment.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (RequestedComment != null)
            {
                var data = new 
                { 
                    comment = RequestedComment.Comment, 
                    username = RequestedComment.User.Username, 
                    fullname = RequestedComment.User.FullName 
                };
                return Succes(null, data);
            }
            
            return Error("Böyle bir yorum yok", null, null, 404);
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
        [ValidateAntiForgeryToken]
        [HttpPost("")]
        public async Task<IActionResult> PostAdd(CommentModel model) //doing comment or reply
        {
            if (model.NeedId == null && model.RelatedCommentId == null || model.NeedId == Guid.Empty && model.RelatedCommentId == null)
            {
                return Error("Yorum yapmak istediğiniz kampanyayı veya cevaplamak istedğiniz yorumu kontrol edin");
            }
            if (!ModelState.IsValid)
            {
                return Error(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }

            if (model.NeedId != Guid.Empty && model.NeedId != null) // add new comment
            {
                var commentedNeed = await Context.Need.FirstOrDefaultAsync(x => x.Id == model.NeedId && !x.IsRemoved);

                if (commentedNeed != null)
                {
                    if (commentedNeed.Stage != 3)
                    {
                        return Error("Burada tartışma başlatılamaz");
                    }

                    var NewComment = new NeedComment
                    {
                        Comment = model.Comment,
                        NeedId = (Guid)model.NeedId,
                        UserId = Guid.Parse(User.Identity.GetUserId())
                    };

                    await Context.AddAsync(NewComment);
                    await Context.SaveChangesAsync();
                    return Succes(null, NewComment.Id, 201);
                }

                return Error("Tartışmanın başlatılacağı kampanya yok",null,null,404);
            }
            else  //[reply] add relational comment
            {
                var repliedComment = await Context.NeedComment.Include(comment => comment.Need).FirstOrDefaultAsync(x => x.Id == model.RelatedCommentId && !x.IsRemoved && !x.Need.IsRemoved);

                if (repliedComment != null)
                {
                    if (repliedComment.Need.Stage != 3)
                    {
                        return Error("Burada cevap yazılamaz");
                    }

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
                        return Succes(null, NewReply.Id, 201);
                    }
                    else
                    {
                        return Error(null, null, null, 1001);
                    }
                }

                return Error("Cevaplanacak yorum yok", null, null, 404);
            }
        }

        [HttpPatch("remove/{Id}")]
        public async Task<IActionResult> PatchRemove(Guid Id)
        {
            if (Id == null || Id == Guid.Empty)
            {
                return Error("Silmek için yorum seçmediniz","Id is required");
            }

            var DeletedComment = await Context.NeedComment.FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved && x.UserId == Guid.Parse(User.Identity.GetUserId()));
            if (DeletedComment != null)
            {
                DeletedComment.IsRemoved = true;
                DeletedComment.UserId = null; // hangi user'ın bu veriyi girdiği boş bırakılmalı
                DeletedComment.Comment = ""; // aynı şekilde içerik yok edilmeli
                await Context.SaveChangesAsync();

                return Succes("Yorumunuz silindi");
            }
            
            return Error("Silmeye çalıştığınız yorum yok",null, null, 404);
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
            if (!ModelState.IsValid)
            {
                return Error(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }

            var EditedComment = await Context.NeedComment.Include(comment => comment.Need).FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsRemoved && x.UserId == Guid.Parse(User.Identity.GetUserId()));
            if (EditedComment != null)
            {
                if(EditedComment.Need.Stage != 3)
                {
                    return Error("Burada yorum düzenlenemez");
                }

                if (EditedComment.Comment != model.Comment)
                {
                    EditedComment.Comment = model.Comment;
                    await Context.SaveChangesAsync();
                    return Succes("Yorum içeriğiniz düzenlendi");
                }
                
                return Error("Aynı içerik ile düzenlemeye çalıştınız");
            }
            return Error("Düzenlemeye çalıştığınız yorum yok",null, null, 404);
        }
    }
}