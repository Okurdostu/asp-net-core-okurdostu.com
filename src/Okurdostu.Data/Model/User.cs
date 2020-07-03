using System;
using System.Collections.Generic;

namespace Okurdostu.Data.Model
{
    public partial class User
    {
        public User()
        {
            IsEmailConfirmed = false;
            IsActive = true;
            CreatedOn = DateTime.Now;

            Need = new HashSet<Need>();
            NeedLike = new HashSet<NeedLike>();
            UserEducation = new HashSet<UserEducation>();
            NeedComment = new HashSet<NeedComment>();
        }

        public long Id { get; private set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Biography { get; set; }
        public string Telephone { get; set; }
        public string PictureUrl { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Twitter { get; set; }
        public string Github { get; set; }
        public string ContactEmail { get; set; }

        public virtual ICollection<Need> Need { get; set; }
        public virtual ICollection<NeedLike> NeedLike { get; set; }
        public virtual ICollection<UserEducation> UserEducation { get; set; }
        public virtual ICollection<NeedComment> NeedComment { get; set; }
    }
}
