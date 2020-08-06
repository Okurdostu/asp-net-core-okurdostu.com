using Okurdostu.Data;

namespace Okurdostu.Web.Extensions
{
    public static class AppUserExtensions
    {
        public static bool PasswordCheck(this User user, string ConfirmPassword)
        {
            return ConfirmPassword.SHA512() == user.Password;
        }
    }
}
