using System;
using System.Collections.Generic;

namespace Okurdostu.Data
{
    public partial class UserEducation
    {
        public UserEducation()
        {
            Id = Guid.NewGuid();

            CreatedOn = DateTime.Now;
            IsRemoved = false;
            IsSentToConfirmation = false;
            IsActiveEducation = false;
            IsConfirmed = false;

            UserEducationDoc = new HashSet<UserEducationDoc>();
        }

        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
        public Guid UniversityId { get; set; }
        public string Department { get; set; }
        public string ActivitiesSocieties { get; set; }
        public string StartYear { get; set; }
        public string EndYear { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsSentToConfirmation { get; set; }
        public bool IsActiveEducation { get; set; }
        public bool IsConfirmed { get; set; }

        public virtual ICollection<UserEducationDoc> UserEducationDoc { get; set; }
        public virtual University University { get; set; }
        public virtual User User { get; set; }
    }
}
