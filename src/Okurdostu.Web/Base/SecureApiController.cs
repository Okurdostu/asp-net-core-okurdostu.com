using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Okurdostu.Web.Base
{
    [Authorize]
    public class SecureApiController : ApiController
    {
    }
}
