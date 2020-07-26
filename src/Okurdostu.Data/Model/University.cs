using System;
using System.Collections.Generic;

namespace Okurdostu.Data
{
    public partial class University
    {
        public University()
        {
            Id = Guid.NewGuid();
            UserEducation = new HashSet<UserEducation>();
        }

        public Guid Id { get; private set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string FriendlyName { get; set; }

        public virtual ICollection<UserEducation> UserEducation { get; set; }
    }
}
