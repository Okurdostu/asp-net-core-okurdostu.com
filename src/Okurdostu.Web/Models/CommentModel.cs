using System;
using System.ComponentModel.DataAnnotations;

namespace Okurdostu.Web.Models
{
    public class CommentModel
    {
        public long? NeedId { get; set; }
        public Guid? RelatedCommentId { get; set; }

        [Required(ErrorMessage = "Bir şeyler yazmalısın")]
        [MaxLength(100, ErrorMessage = "En fazla 100 karakter")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }
    }
}