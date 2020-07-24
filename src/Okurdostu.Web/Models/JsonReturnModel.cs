using Newtonsoft.Json;

namespace Okurdostu.Web.Models
{
    public class JsonReturnModel
    {
        public bool Status { get; set; }

        public string MessageTitle { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }
    }
}
