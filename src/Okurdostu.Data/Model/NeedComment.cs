using System;

namespace Okurdostu.Data.Model
{
    public partial class NeedComment
    {
        public NeedComment()
        {
            Id = Guid.NewGuid();
            IsRemoved = false;
            CreatedOn = DateTime.Now;
        }

        public Guid Id { get; set; }
        public string Comment { get; set; }
        public Guid? RelatedMainCommentId { get; set; }
        public Guid? RelatedCommentId { get; set; }
        public long NeedId { get; set; }
        public long UserId { get; set; }
        public bool IsRemoved { get; set; }
        public DateTime? CreatedOn { get; set; }

        public virtual Need Need { get; set; }
        public virtual User User { get; set; }
    }
}
