using System;

namespace Okurdostu.Data
{
    public partial class Feedback
    {
        public Feedback()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTime.Now;
        }

        public Guid Id { get; private set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
