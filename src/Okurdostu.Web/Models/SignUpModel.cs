using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Models
{
    public class SignUpModel
    {
        [Required(ErrorMessage = "E-mail adresinizi yazmalısınız")]
        [Display(Name = "Email adresi")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Lütfen geçerli bir e-mail adresi olsun")]
        [MaxLength(25, ErrorMessage = "Çok uzun, en fazla 50 karakter")]
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
        [RegularExpression(@"[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+", ErrorMessage = "Gerçekten adınız ve soyadınız da harf dışında karakter mı var?")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Kendinize bir kullanıcı adı seçmelisiniz")]
        [Display(Name = "Kullanıcı adı")]
        [MaxLength(15, ErrorMessage = "Çok uzun, en fazla 15 karakter")]
        [MinLength(3, ErrorMessage = "Çok kısa, en az 3 karakter")]
        [RegularExpression(@"[0-9a-z]+", ErrorMessage = "Sadece küçük harflerle latin karakterler ve rakamlar")]
        public string Username { get; set; }
    }
}
