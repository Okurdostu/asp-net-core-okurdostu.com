using System.ComponentModel.DataAnnotations;

namespace Okurdostu.Web.Models
{
    public class NeedModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Bir başlık yazmalısınız")]
        [MaxLength(75, ErrorMessage = "Başlık en fazla 75 karakter olmalı")]
        [RegularExpression(@"[a-zA-ZğüşıöçĞÜŞİÖÇ\s,?!]+", ErrorMessage = "A'dan Z'ye harfler, boşluk, virgül, soru işareti ve ünlem girişi yapabilirsiniz.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Bir açıklama yazmalısınız")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}
