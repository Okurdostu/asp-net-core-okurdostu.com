using System;
using System.Collections.Generic;

namespace Okurdostu.Data
{
    public partial class User
    {
        public User()
        {
            Id = Guid.NewGuid();
            IsEmailConfirmed = false;
            IsActive = true;
            CreatedOn = DateTime.Now;
            LastChangedOn = DateTime.Now;

            Need = new HashSet<Need>();
            NeedComment = new HashSet<NeedComment>();
            NeedLike = new HashSet<NeedLike>();
            UserEducation = new HashSet<UserEducation>();
            UserEmailConfirmation = new HashSet<UserEmailConfirmation>();
            UserPasswordReset = new HashSet<UserPasswordReset>();
        }

        public Guid Id { get; private set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Biography { get; set; }
        public string Telephone { get; set; }
        public string PictureUrl { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Twitter { get; set; }
        public string Github { get; set; }
        public string ContactEmail { get; set; }
        public DateTime LastChangedOn { get; set; }

        public DateTime? BirthDate { get; set; }
        public bool? AreBDMonthDayPublic { get; set; }
        public bool? IsBDYearPublic { get; set; }

        public string BirthDateMonthName()
        {
            string[] monthNames = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
            return BirthDate != null ? monthNames[BirthDate.Value.Month - 1] : null;
        }

        public virtual ICollection<Need> Need { get; set; }
        public virtual ICollection<NeedComment> NeedComment { get; set; }
        public virtual ICollection<NeedLike> NeedLike { get; set; }
        public virtual ICollection<UserEducation> UserEducation { get; set; }
        public virtual ICollection<UserEmailConfirmation> UserEmailConfirmation { get; set; }
        public virtual ICollection<UserPasswordReset> UserPasswordReset { get; set; }
    }
}
