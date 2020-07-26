using HtmlAgilityPack;
using System.Net;
using System.Threading;

namespace Okurdostu.Web.Models.NeedItem
{
    public class Pandora : NeedItemModel
    {
        public string Picture { get; set; }
        public string DiscountedPrice { get; set; }
        public string NormalPrice { get; set; }
        public Pandora Product(string url)
        {
            try
            {
                var link = @url;
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDocument = web.Load(link);
                if (htmlDocument.DocumentNode.SelectSingleNode("//img[@src='/images/hata.png']") != null)
                {
                    Error = "Sayfa bulunamadı";
                    return this;
                }

                var BookNameNode = htmlDocument.DocumentNode.SelectSingleNode(".//div[@id='urun-detay']/h1");
                if (BookNameNode != null)
                {
                    Name = BookNameNode.InnerText;
                }
                else
                {
                    Error = "Kitap adına ulaşılamadı";
                    return this;
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
                var indirimliFiyatHtmlNode = htmlDocument.DocumentNode.SelectSingleNode(".//ul[@class='borderedPhone']/li/strong[@class='indirimliFiyat']");
                var eskiFiyatHtmlNode = htmlDocument.DocumentNode.SelectSingleNode(".//ul[@class='borderedPhone']/li/span[@class='eskiFiyat']");

                if (indirimliFiyatHtmlNode != null && eskiFiyatHtmlNode != null)
                {
                    DiscountedPrice = indirimliFiyatHtmlNode.InnerText;
                    NormalPrice = eskiFiyatHtmlNode.InnerText;

                    DiscountedPrice = DiscountedPrice.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace(",", ".").Replace("TL", "");
                    NormalPrice = NormalPrice.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace(",", ".").Replace("TL", "");
                }
                else if (indirimliFiyatHtmlNode != null)
                {
                    NormalPrice = indirimliFiyatHtmlNode.InnerText;
                    NormalPrice = NormalPrice.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace(",", ".").Replace("TL", "");
                }
                else
                {
                    Error = "Kitabın fiyatına ulaşılamadı";
                    return this;
                }

                var PictureNode = htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='coverWrapper posRel']/img");
                if (PictureNode != null)
                {
                    Picture = "https://www.pandora.com.tr" + PictureNode.Attributes["src"].Value;
                }
                else
                {
                    Picture = "";
                }

                Link = url;
                return SelectPrice(this);
            }
            catch
            {
                Error = "Beklenmedik bir problem ile karşılaştık";
                return this;
            }
        }

        public Pandora SelectPrice(Pandora pandoraProduct)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            double _Price;
            /// <summary>
            /// Product.NormalPrice ve Product.DiscountedPrice var
            /// Eğer ki NormalPrice varsa bunu alıp İndirimli fiyata uğramıyoruz
            /// Sebebiyse Ürünün indirimden çıkabileceğini göz önünde bulundurup Kampanya'yı fiyatlandırmalıyız.
            /// </summary>

            if (pandoraProduct.NormalPrice.Length > 0)
            {
                _Price = double.Parse(pandoraProduct.NormalPrice);
            }
            else
            {
                _Price = double.Parse(pandoraProduct.DiscountedPrice);
            }

            //if (_Price < pandoraUcretsizKargo)
            //    _Price += pandoraKargoUcreti;

            pandoraProduct.Price = _Price;
            return pandoraProduct;
        }
    }
}