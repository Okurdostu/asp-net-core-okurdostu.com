using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Okurdostu.Web.Models
{
    public class FeedbackModel
    {
        [Required(ErrorMessage = "Lütfen mesajı boş bırakmayınız")]
        [Display(Name = "Mesajınız")]
        [DataType(DataType.MultilineText)]
        [MaxLength(2000, ErrorMessage = "En fazla 2000 karakter")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Lütfen mail adresinizi boş bırakmayınız")]
        [Display(Name = "E-mail adresiniz")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Lütfen geçerli bir e-mail adresi olsun")]
        [MaxLength(100, ErrorMessage = "En fazla 100 karakter")]
        public string Email { get; set; }
    }
}