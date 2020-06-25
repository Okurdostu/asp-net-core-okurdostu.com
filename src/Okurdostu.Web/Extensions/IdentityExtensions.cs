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
            return claim != null ? claim.Value : null;
        }
    }
}
