namespace Okurdostu.Web.Services
{
    public interface IEmailConfiguration
    {
        string Server { get; }
        int Port { get; }
        string Password { get; set; }
    }

    public class EmailConfiguration : IEmailConfiguration
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
    }
}
