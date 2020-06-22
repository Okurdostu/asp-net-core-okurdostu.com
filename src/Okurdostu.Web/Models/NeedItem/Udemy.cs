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
        public decimal Price { get; private set; }

        public Udemy Product(string url)
        {
            Udemy Product = new Udemy();
            try
            {
                var link = @url;
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDocument = web.Load(link);
                if (htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='title']").Attributes["content"].Value != null)
                    Product.Name = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='title']").Attributes["content"].Value;
                else
                {
                    Product.Error = "Kursun adına ulaşılamadı";
                    return Product;
                }

                if (htmlDocument.DocumentNode.SelectSingleNode("//span[contains(@class,'price-text__current')]") != null)
                    Product.DiscountedPrice = WebUtility.HtmlDecode(htmlDocument.DocumentNode.SelectSingleNode("//span[contains(@class,'price-text__current')]").InnerText).Replace("Current price", "").Replace(":", "").Replace(" ", "").Replace("₺", "").Replace("\n", "").Replace("\r", "").Replace(".", ",");
                else if (htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='udemy_com:price']").Attributes["content"].Value != null)
                    Product.NormalPrice = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='udemy_com:price']").Attributes["content"].Value.Replace("₺", "").Replace(".", ",");
                
                if (Product.DiscountedPrice == null && Product.NormalPrice == null)
                {
                    Product.Error = "Kursun fiyatına ulaşılamadı";
                    return Product;
                }
                else
                    Product.Price = Product.DiscountedPrice != null ? decimal.Parse(Product.DiscountedPrice) : decimal.Parse(Product.NormalPrice);

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