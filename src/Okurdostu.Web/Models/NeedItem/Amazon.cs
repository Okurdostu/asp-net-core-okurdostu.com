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

            try
            {
                var link = @url;
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDocument = web.Load(link);
                if (htmlDocument.DocumentNode.SelectSingleNode("//a[@id='bylineInfo']") != null)
                    Product.Name = htmlDocument.DocumentNode.SelectSingleNode("//a[@id='bylineInfo']").InnerText;
                else
                {
                    Product.Error = "Markaya ulaşılamadı";
                    return Product;
                }

                if (htmlDocument.DocumentNode.SelectSingleNode("//span[@class='priceBlockStrikePriceString a-text-strike']") != null)
                    Product.Price = decimal.Parse(htmlDocument.DocumentNode.SelectSingleNode("//span[@class='priceBlockStrikePriceString a-text-strike']").InnerText.Replace(".", "").Replace(",", ".").Replace("₺", "").Replace(" ", ""));
                else if (htmlDocument.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']") != null)
                    Product.Price = decimal.Parse(htmlDocument.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']").InnerText.Replace(".", "").Replace(",", ".").Replace("₺", "").Replace(" ", ""));
                else
                    Product.Error = "Fiyatına ulaşılamadı";


                Product.Link = url;

                return Product;
            }
            catch (Exception)
            {
                Product.Error = "Beklenmedik bir hata ile karşılaştık.";
                return Product;
            }

        }

    }
}