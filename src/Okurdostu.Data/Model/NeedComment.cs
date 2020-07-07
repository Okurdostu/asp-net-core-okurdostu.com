using Okurdostu.Data.Model;
using System;
using System.Collections.Generic;

namespace Okurdostu.Data.Model
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

        public Guid Id { get; set; }
        public string Comment { get; set; }
        public Guid? RelatedCommentId { get; set; }
        public long NeedId { get; set; }
        public long? UserId { get; set; }
        public bool IsRemoved { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual Need Need { get; set; }
        public virtual NeedComment RelatedComment { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<NeedComment> InverseRelatedComment { get; set; }
    }
}
