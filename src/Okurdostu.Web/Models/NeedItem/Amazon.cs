using HtmlAgilityPack;
using System;

namespace Okurdostu.Web.Models.NeedItem
{
    public class Amazon
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public decimal Price { get; private set; }
        public string Error { get; private set; }
        public Amazon Product(string url)
        {
            Amazon Product = new Amazon();
            var link = @url;
            HtmlWeb web = new HtmlWeb();
            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36";
            HtmlDocument htmlDocument = web.Load(link);
            if (htmlDocument.DocumentNode.SelectSingleNode("//a[@id='bylineInfo']") != null)
                Product.Name = htmlDocument.DocumentNode.SelectSingleNode("//a[@id='bylineInfo']").InnerText;
            else
            {
                Product.Error = "Markaya ulaşılamadı";
                return Product;
            }

            if (htmlDocument.DocumentNode.SelectSingleNode("//span[@class='priceBlockStrikePriceString a-text-strike']") != null)
                Product.Price = decimal.Parse(htmlDocument.DocumentNode.SelectSingleNode("//span[@class='priceBlockStrikePriceString a-text-strike']").InnerText.Replace(",", ".").Replace("₺", "").Replace(" ", ""));
            else if (htmlDocument.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']") != null)
                Product.Price = decimal.Parse(htmlDocument.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']").InnerText.Replace(",", ".").Replace("₺", "").Replace(" ", ""));
            else
                Product.Error = "Fiyatına ulaşılamadı";

            if (Product.Price == 0)
                Product.Error = "Fiyatına ulaşılamadı";

            Product.Link = url;

            return Product;
        }

    }
}