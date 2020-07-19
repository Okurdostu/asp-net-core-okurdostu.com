using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Models.NeedItem
{
    public class NeedItemModel
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public double Price { get; set; }
        public string Error { get; set; }
    }
}
