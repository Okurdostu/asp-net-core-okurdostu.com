using HtmlAgilityPack;

namespace Okurdostu.Web.Models.NeedItem
{
    public class Udemy : NeedItemModel
    {
        public Udemy Product(string url)
        {
            try
            {
                var link = @url;
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDocument = web.Load(link);

                var UdemyCourseName = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='title']").Attributes["content"].Value;

                if (UdemyCourseName != null)
                {
                    Name = UdemyCourseName;
                }
                else
                {
                    Error = "Kursun adına ulaşılamadı";
                    return this;
                }

                Price = 0;
                //udemy course prices aren't stable, each refresh give another value.
                //for now, it need custom control. i thought when need campaign is confirming on panel we can input a stable price value.
                Link = url;
            }
            catch (System.Exception)
            {
                Error = "Beklenmedik bir problem ile karşılaştık";
            }

            return this;
        }
    }
}