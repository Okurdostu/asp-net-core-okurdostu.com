using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Okurdostu.Web.Base
{
    [Authorize]
    public class SecureApiController : ApiController
    {
        [NonAction]
        public bool DeleteFileFromServer(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
