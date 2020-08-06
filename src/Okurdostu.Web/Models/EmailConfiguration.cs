using Okurdostu.Web.Services;

namespace Okurdostu.Web.Models
{
    public class EmailConfiguration : IEmailConfigurationService
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
    }
}
