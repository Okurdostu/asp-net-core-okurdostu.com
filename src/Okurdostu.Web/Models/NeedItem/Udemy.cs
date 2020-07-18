using HtmlAgilityPack;
using System.Net;

namespace Okurdostu.Web.Models.NeedItem
{
    public class Udemy
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public string Error { get; private set; }
        public string DiscountedPrice { get; set; }
        public string NormalPrice { get; set; }
        public double Price { get; private set; }

        public Udemy Product(string url)
        {
            Udemy Product = new Udemy();
            try
            {
                var link = @url;
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDocument = web.Load(link);
                if (htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='title']").Attributes["content"].Value != null)
                {
                    Product.Name = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='title']").Attributes["content"].Value;
                }
                else
                {
                    Product.Error = "Kursun adına ulaşılamadı";
                    return Product;
                }
                Price = 0;
                //udemy course prices aren't stable, each refresh give another value.
                //for now, it need custom control. i thought when need campaign is confirming on panel we can input a stable price value.
                Product.Link = url;
            }
            catch (System.Exception)
            {
                Product.Error = "Beklenmedik bir problem ile karşılaştık";
            }
            return Product;
        }
    }
}