using HtmlAgilityPack;
using System;

namespace Okurdostu.Web.Models.NeedItem
{
    public class Amazon : NeedItemModel
    {
        public Amazon Product(string url)
        {
            try
            {
                var link = @url;
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDocument = web.Load(link);

                var BrandNode = htmlDocument.DocumentNode.SelectSingleNode("//a[@id='bylineInfo']");

                if (BrandNode != null)
                {
                    Name = BrandNode.InnerText;
                }
                else
                {
                    Error = "Markaya ulaşılamadı";
                    return this;
                }

                var DiscountedPriceNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']");
                var NormalPriceNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='priceBlockStrikePriceString a-text-strike']");

                if (NormalPriceNode != null)
                {
                    Price = double.Parse(NormalPriceNode.InnerText.Replace(".", "").Replace(",", ".").Replace("₺", "").Replace(" ", ""));
                }
                else if (DiscountedPriceNode != null)
                {
                    Price = double.Parse(DiscountedPriceNode.InnerText.Replace(".", "").Replace(",", ".").Replace("₺", "").Replace(" ", ""));
                }
                else
                {
                    Error = "Ürünün fiyatına ulaşılamadı";
                }

                Link = url;

                return this;
            }
            catch (Exception)
            {
                Error = "Beklenmedik bir hata ile karşılaştık.";
                return this;
            }

        }
    }
}