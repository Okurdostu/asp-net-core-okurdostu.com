using System;

namespace Okurdostu.Data
{
    public partial class Feedback
    {
        public Feedback()
        {
            CreatedOn = DateTime.Now;
        }
        public long Id { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime? CreatedOn { get; private set; }
    }
}
