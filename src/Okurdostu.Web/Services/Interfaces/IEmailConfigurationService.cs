namespace Okurdostu.Web.Services
{
    public interface IEmailConfigurationService
    {
        string Server { get; }
        int Port { get; }
        string Password { get; set; }
    }
}
