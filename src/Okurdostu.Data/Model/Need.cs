using System;
using System.Collections.Generic;

namespace Okurdostu.Data
{
    public partial class Need
    {
        public Need()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            Id = Guid.NewGuid();
            CreatedOn = DateTime.Now;
            IsSentForConfirmation = false;
            IsCompleted = false;
            IsConfirmed = false;
            IsRemoved = false;
            IsWrong = false;
            TotalCharge = 0;
            TotalCollectedMoney = 0;

            NeedComment = new HashSet<NeedComment>();
            NeedItem = new HashSet<NeedItem>();
            NeedLike = new HashSet<NeedLike>();
        }

        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string FriendlyTitle { get; set; }
        public string Description { get; set; }
        public decimal? TotalCollectedMoney { get; set; }
        public decimal? TotalCharge { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsSentForConfirmation { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CreatedOn { get; private set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        public DateTime? LastCheckOn { get; set; }
        public bool IsWrong { get; set; }
       public sbyte Stage
        {
            get
            {
                if (IsCompleted)
                    return 4; // tamamlanmış, para toplaması kapatılmış

                if (IsConfirmed)
                    return 3; // onaylanmış - sergilenen - kampanya için para toplama durumunda olan

                if (IsSentForConfirmation)
                    return 2; // onay için yollanmış

                return 1; // yeni oluşturulmuş: onay için yollanmamış
            }
        }
        public bool ShouldBeCheck
        {
            get
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                var PassedTime = DateTime.Now - LastCheckOn;
                return LastCheckOn == null || PassedTime.Value.TotalMinutes > 60 && Stage > 1 && Stage < 4;
            }
        }
        public string Link
        {
            get
            {
                return User.Username + "/ihtiyac/" + FriendlyTitle;
            }
        }
        public decimal? CompletedPercentage => 100 - (TotalCharge - TotalCollectedMoney) * 100 / TotalCharge;
        public virtual User User { get; set; }
        public virtual ICollection<NeedComment> NeedComment { get; set; }
        public virtual ICollection<NeedItem> NeedItem { get; set; }
        public virtual ICollection<NeedLike> NeedLike { get; set; }
    }
}
