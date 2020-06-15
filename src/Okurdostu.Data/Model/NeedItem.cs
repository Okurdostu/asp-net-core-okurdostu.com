using System;
using System.Collections.Generic;

namespace Okurdostu.Data.Model
{
    public partial class NeedItem
    {
        public long Id { get; set; }
        public long NeedId { get; set; }
        public string Link { get; set; }
        public string Picture { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int PlatformName { get; set; }
        public bool IsWrong { get; set; }
        public bool IsRemoved { get; set; }

        public virtual Need Need { get; set; }
    }
}
