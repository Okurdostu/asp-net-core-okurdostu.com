using Microsoft.AspNetCore.Authorization;

namespace Okurdostu.Web.Base
{
    [Authorize]
    public class SecureApiController : ApiController
    {
    }
}
