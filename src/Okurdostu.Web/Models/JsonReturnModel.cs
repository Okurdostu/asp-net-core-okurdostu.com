using Newtonsoft.Json;

namespace Okurdostu.Web.Models
{
    public class JsonReturnModel
    {
        public bool Succes { get; set; }
        public int Code { get; set; } //like 200, 403, 400..
        public string InternalMessage { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
