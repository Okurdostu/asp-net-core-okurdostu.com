using System;
using System.ComponentModel.DataAnnotations;

namespace Okurdostu.Data
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
        public Guid UserId { get; set; }
        public string NewEmail { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UsedOn { get; set; }

        public virtual User User { get; set; }
    }
}
