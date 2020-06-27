using System;
using System.ComponentModel.DataAnnotations;

namespace Okurdostu.Data.Model
{
    public partial class UserEmailConfirmation
    {
        public UserEmailConfirmation()
        {
            GUID = new Guid();
            CreatedOn = DateTime.Now;
            IsUsed = false;
        }

        [Key]
        public Guid GUID { get; private set; }
        public long UserId { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? CreatedOn { get; private set; }
        public DateTime? UsedOn { get; set; }


        public virtual User User { get; set; }
    }
}
