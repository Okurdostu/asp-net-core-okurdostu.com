using System;
using System.Collections.Generic;

namespace Okurdostu.Data.Model
{
    public partial class NeedLike
    {
        public long Id { get; set; }
        public long NeedId { get; set; }
        public long UserId { get; set; }
        public bool IsCurrentLiked { get; set; }

        public virtual Need Need { get; set; }
        public virtual User User { get; set; }
    }
}
