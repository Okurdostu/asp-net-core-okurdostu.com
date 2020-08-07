using System.ComponentModel.DataAnnotations;

namespace Okurdostu.Web.Models
{
    public class ProfileModel //all inputs about user profile
    {
        [Required(ErrorMessage = "Kullanıcı adınızı yazmalısınız")]
        [Display(Name = "Kullanıcı adı")]
        [MaxLength(15, ErrorMessage = "Çok uzun, en fazla 15 karakter")]
        [MinLength(3, ErrorMessage = "Çok kısa")]
        [RegularExpression(@"[0-9a-z]+", ErrorMessage = "Sadece küçük harflerle latin karakterler ve rakamlar")]
        public string Username { get; set; }

        [Required(ErrorMessage = "E-mail adresinizi yazmalısınız")]
        [Display(Name = "Email adresi")]
        [MaxLength(25, ErrorMessage = "Çok uzun, en fazla 25 karakter")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Lütfen geçerli bir e-mail adresi olsun")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola seçmelisiniz")]
        [Display(Name = "Parola")]
        [DataType(DataType.Password)]
        [MinLength(7, ErrorMessage = "En az 7 karakterden oluşan bir şifre oluşturun")]
        [MaxLength(30, ErrorMessage = "Çok uzun, en fazla 30 karakter")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Adınızı ve soyadınızı yazmalısınız")]
        [Display(Name = "Adınız ve soyadınız")]
        [MaxLength(50, ErrorMessage = "Çok uzun, en fazla 50 karakter")]
        [MinLength(5, ErrorMessage = "Çok kısa")]
        [RegularExpression(@"[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+", ErrorMessage = "Gerçekten adınız ve soyadınız da harf dışında karakter mı var?")]
        public string FullName { get; set; }

        [Display(Name = "Hakkında")]
        [MaxLength(256, ErrorMessage = "Lütfen en fazla 256 karakter giriniz.")]
        [DataType(DataType.MultilineText)]
        public string Biography { get; set; }

        [Display(Name = "Twitter")]
        [MaxLength(15, ErrorMessage = "Lütfen en fazla 15 karakter giriniz.")]
        [DataType(DataType.Text)]
        public string Twitter { get; set; }

        [Display(Name = "Github")]
        [MaxLength(39, ErrorMessage = "Lütfen en fazla 39 karakter giriniz.")]
        [DataType(DataType.Text)]
        public string Github { get; set; }

        [Display(Name = "E-mail")]
        [MaxLength(50, ErrorMessage = "Lütfen en fazla 50 karakter giriniz.")]
        [DataType(DataType.EmailAddress)]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "Kimliğinizi doğrulamak için şuan ki parolanızı girmelisiniz")]
        [Display(Name = "Onay parolası")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Yeni parola onay")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Parolarınız uyuşmuyor")]
        public string RePassword { get; set; }
    }
}
