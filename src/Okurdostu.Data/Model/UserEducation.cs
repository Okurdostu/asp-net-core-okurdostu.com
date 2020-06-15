using System;
using System.Collections.Generic;

namespace Okurdostu.Data.Model
{
    public partial class UserEducation
    {
        public UserEducation()
        {
            UserEducationDocument = new HashSet<UserEducationDocument>();
        }

        public long Id { get; set; }
        public Guid UserId { get; set; }
        public int UniversityId { get; set; }
        public string Department { get; set; }
        public string ActivitiesSocieties { get; set; }
        public decimal? StartYear { get; set; }
        public decimal? EndYear { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsSentToConfirmation { get; set; }
        public bool IsActiveEducation { get; set; }

        public virtual University University { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<UserEducationDocument> UserEducationDocument { get; set; }
    }
}
