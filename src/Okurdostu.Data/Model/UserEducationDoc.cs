using System;
using System.Collections.Generic;

namespace Okurdostu.Data.Model
{
    public partial class UserEducationDoc
    {
        public long Id { get; set; }
        public long UserEducationId { get; set; }
        public string PathAfterRoot { get; set; }
        public string FullPath { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual UserEducation UserEducation { get; set; }
    }
}
