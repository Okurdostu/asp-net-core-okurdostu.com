using System;
using System.Collections.Generic;

namespace Okurdostu.Data.Model
{
    public partial class University
    {
        public University()
        {
            UserEducation = new HashSet<UserEducation>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string FriendlyName { get; set; }

        public virtual ICollection<UserEducation> UserEducation { get; set; }
    }
}
