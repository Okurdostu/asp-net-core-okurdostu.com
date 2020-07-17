using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Okurdostu.Web
{
    public static class IdentityExtensions
    {
        public static string GetUserId(this IIdentity identity)
        {
            ClaimsIdentity CI = identity as ClaimsIdentity;
            Claim claim = CI?.FindFirst("Id");
            return claim?.Value;
        }

        public static string GetUsername(this IIdentity identity)
        {
            ClaimsIdentity CI = identity as ClaimsIdentity;
            Claim claim = CI?.FindFirst("Username");
            return claim?.Value;
        }

        public static string GetEmail(this IIdentity identity)
        {
            ClaimsIdentity CI = identity as ClaimsIdentity;
            Claim claim = CI?.FindFirst("Email");
            return claim?.Value;
        }
        public static bool GetEmailConfirmedState(this IIdentity identity)
        {
            ClaimsIdentity CI = identity as ClaimsIdentity;
            Claim claim = CI?.FindFirst("EmailState");

            return claim?.Value == "True";
        }
        public static string GetPhoto(this IIdentity identity)
        {
            ClaimsIdentity CI = identity as ClaimsIdentity;
            Claim claim = CI?.FindFirst("Photo");
            return claim?.Value;
        }
    }
}
