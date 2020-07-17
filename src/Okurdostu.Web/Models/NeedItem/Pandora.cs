using HtmlAgilityPack;
using System.Net;

namespace Okurdostu.Web.Models.NeedItem
{
    public class Pandora
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public decimal Price { get; private set; }
        public string Picture { get; set; }
        public string Error { get; private set; }
        public string DiscountedPrice { get; set; }
        public string NormalPrice { get; set; }
        public Pandora Product(string url)
        {
            Pandora Product = new Pandora();
            try
            {
                var link = @url;
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDocument = web.Load(link);
                if (htmlDocument.DocumentNode.SelectSingleNode("//img[@src='/images/hata.png']") != null)
                {
                    Product.Error = "Sayfa bulunamadı";
                    return Product;
                }
                if (htmlDocument.DocumentNode.SelectSingleNode(".//div[@id='urun-detay']/h1") != null)
                    Product.Name = WebUtility.HtmlDecode(htmlDocument.DocumentNode.SelectSingleNode(".//div[@id='urun-detay']/h1").InnerText);
                else
                {
                    Product.Error = "Kitap adına ulaşılamadı";
                    return Product;
                }
                /// <summary>
                /// indirimli üründe iki tip geri dönüş var: strong[@class='indirimliFiyat'] ve span[@class='eskiFiyat']
                /// indirimsiz üründe bir tip geri dönüş var strong[@class='indirimliFiyat'] (ürün indirimli olmasa bile): strong[@class='indirimliFiyat'] buradan geri dönüş alıyoruz.
                /// 
                /// 
                /// Durum 1: Sayfa'da iki tane fiyat varsa (ürün indirimli ise)
                /// DiscountedPrice'a indirimli(strong[@class='indirimliFiyat']),
                /// NormalPrice'a indirimsiz(span[@class='eskiFiyat']) halini al
                /// 
                /// Durum 2: Sayfa'da bir tane fiyat varsa (Ürün indirimsiz ise)
                /// NormalPrice'a fiyatı(strong[@class='indirimliFiyat']) al
                /// 
                /// Durum 3: zaten açık
                /// </summary>
                if (htmlDocument.DocumentNode.SelectSingleNode(".//ul[@class='borderedPhone']/li/strong[@class='indirimliFiyat']") != null && htmlDocument.DocumentNode.SelectSingleNode(".//ul[@class='borderedPhone']/li/span[@class='eskiFiyat']") != null)
                {
                    Product.DiscountedPrice = WebUtility.HtmlDecode(htmlDocument.DocumentNode.SelectSingleNode(".//ul[@class='borderedPhone']/li/strong[@class='indirimliFiyat']").InnerText.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace(",", ".").Replace("TL", ""));
                    Product.NormalPrice = WebUtility.HtmlDecode(htmlDocument.DocumentNode.SelectSingleNode(".//ul[@class='borderedPhone']/li/span[@class='eskiFiyat']").InnerText.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace(",", ".").Replace("TL", ""));
                }
                else if (htmlDocument.DocumentNode.SelectSingleNode(".//ul[@class='borderedPhone']/li/strong[@class='indirimliFiyat']") != null)
                    Product.NormalPrice = WebUtility.HtmlDecode(htmlDocument.DocumentNode.SelectSingleNode(".//ul[@class='borderedPhone']/li/strong[@class='indirimliFiyat']").InnerText.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace(",", ".").Replace("TL", ""));
                else
                {
                    Product.Error = "Kitabın fiyatına ulaşılamadı";
                    return Product;
                }
                if (htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='coverWrapper posRel']/img") != null)
                    Product.Picture = "https://www.pandora.com.tr" + htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='coverWrapper posRel']/img").Attributes["src"].Value;
                else Product.Picture = "";
                Product.Link = url;
                return SelectPrice(Product);
            }
            catch
            {
                Product.Error = "Beklenmedik bir problem ile karşılaştık";
                return Product;
            }
        }

        public Pandora SelectPrice(Pandora Product)
        {
            decimal _Price;
            /// <summary>
            /// Product.NormalPrice ve Product.DiscountedPrice var
            /// Eğer ki NormalPrice varsa bunu alıp İndirimli fiyata uğramıyoruz
            /// Sebebiyse Ürünün indirimden çıkabileceğini göz önünde bulundurup Kampanya'yı fiyatlandırmalıyız.
            /// </summary>


            if (Product.NormalPrice.Length > 0)
                _Price = decimal.Parse(Product.NormalPrice);
            else
                _Price = decimal.Parse(Product.DiscountedPrice);

            //if (_Price < pandoraUcretsizKargo)
            //    _Price += pandoraKargoUcreti;

            Product.Price = _Price;
            return Product;
        }
    }
}