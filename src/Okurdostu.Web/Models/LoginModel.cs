using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Kullanıcı adı, telefon veya e-mail adresinizi kullanın")]
        [Display(Name = "Kullanıcı adı, telefon veya email")]
        [DataType(DataType.Text)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Parolanızı girmeyi unuttunuz")]
        [Display(Name = "Parola")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
