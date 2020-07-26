using System;
using System.Collections.Generic;

namespace Okurdostu.Data
{
    public partial class NeedLike
    {
        public NeedLike()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid NeedId { get; set; }
        public Guid UserId { get; set; }
        public bool IsCurrentLiked { get; set; }

        public virtual Need Need { get; set; }
        public virtual User User { get; set; }
    }
}
