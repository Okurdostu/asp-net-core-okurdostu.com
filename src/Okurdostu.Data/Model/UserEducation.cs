using System;
using System.Collections.Generic;

namespace Okurdostu.Data.Model
{
    public partial class UserEducation
    {
        public UserEducation()
        {
            CreatedOn = DateTime.Now;
            IsRemoved = false;
            IsSentToConfirmation = false;
            IsActiveEducation = false;
            IsConfirmed = false;
            UserEducationDoc = new HashSet<UserEducationDoc>();
        }

        public long Id { get; set; }
        public long UserId { get; set; }
        public short UniversityId { get; set; }
        public string Department { get; set; }
        public string ActivitiesSocieties { get; set; }
        public string StartYear { get; set; } //manipüle edilebiliyor: editlerken veya yeni eğitim oluştururken 1980'den aşağıya, şuan ki yılın üstünde bir değer girilmesine izin verme
        public string EndYear { get; set; } //manipüle edilebiliyor: editlerken veya yeni eğitim oluştururken 1980'den aşağıya ve (şuan ki yıl + 7) üstünde bir değer girilmesine izin verme
        public DateTime CreatedOn { get; private set; }
        public bool IsRemoved { get; set; }
        public bool IsSentToConfirmation { get; set; }
        public bool IsActiveEducation { get; set; }
        public bool IsConfirmed { get; set; }

        public virtual University University { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<UserEducationDoc> UserEducationDoc { get; set; }
    }
}
