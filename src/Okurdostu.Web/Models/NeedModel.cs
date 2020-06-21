using System.ComponentModel.DataAnnotations;

namespace Okurdostu.Web.Models
{
    public class NeedModel
    {
        [Required]
        [Display(Name = "Başlık")]
        [MaxLength(75, ErrorMessage = "Başlık en fazla 75 karakter olmalı")]
        [RegularExpression(@"[a-zA-ZğüşıöçĞÜŞİÖÇ\s,?!]+", ErrorMessage = "A'dan Z'ye harfler, boşluk, virgül, soru işareti ve ünlem girişi yapabilirsiniz.")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Link")]
        public string ItemLink { get; set; }
    }
}
