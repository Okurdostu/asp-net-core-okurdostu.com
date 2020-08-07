namespace Okurdostu.Web.Models
{
    public class ReturnModel
    {
        public bool Succes { get; set; }
        public int Code { get; set; }
        public string InternalMessage { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
