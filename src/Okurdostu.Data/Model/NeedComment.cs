using System;
using System.Collections.Generic;

namespace Okurdostu.Data
{
    public partial class NeedComment
    {
        public NeedComment()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTime.Now;
            IsRemoved = false;

            InverseRelatedComment = new HashSet<NeedComment>();
        }

        public Guid Id { get; private set; }
        public string Comment { get; set; }
        public Guid? RelatedCommentId { get; set; }
        public Guid NeedId { get; set; }
        public Guid? UserId { get; set; }
        public bool IsRemoved { get; set; }
        public DateTime CreatedOn { get; set; }
        public string HowLongPassed
        {
            get
            {
                var PassedTime = DateTime.Now - CreatedOn;
                if (PassedTime.Days >= 30)
                {
                    string[] MonthNames = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

                    return MonthNames[CreatedOn.Month - 1] + " " + CreatedOn.Year;
                }
                else if (PassedTime.Days >= 1)
                {
                    return PassedTime.Days + " gün önce yazdı";
                }
                else if (PassedTime.Hours >= 1)
                {
                    return PassedTime.Hours + " saat önce yazdı";
                }
                else if (PassedTime.Days < 1 && PassedTime.Hours < 1)
                {
                    if (PassedTime.Minutes == 0)
                    {
                        return "Biraz önce yazdı";
                    }
                    else
                    {
                        return PassedTime.Minutes + " dakika önce yazdı";
                    }
                }
                return "";
            }
        }
        public virtual Need Need { get; set; }
        public virtual NeedComment RelatedComment { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<NeedComment> InverseRelatedComment { get; set; }
    }
}
