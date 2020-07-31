using Okurdostu.Web.Models.NeedItem;
using Xunit;

namespace Okurdostu.Web.Tests
{
    public class NeedItemTests
    {
        //product page url amazon, pandora or udemy
        private string customUrl = "";
        //expected product brand name is for amazon,expected book name is for pandora,expected course name is for udemy
        private string customExpectedName = "";

        private double customExpectedPrice;

        [Fact]
        public void Amazon()
        {
            customUrl = "https://www.amazon.com.tr/Lenovo-30BFS3W400-W-2135-GTX1080-512SSD/dp/B08BG6W56C?ref_=s9_apbd_orecs_hd_bw_bDkqJAl&pf_rd_r=7G8HYQ84V5WC1RQSETBV&pf_rd_p=f9a1563d-4e14-5798-a39a-e461d59b63a6&pf_rd_s=merchandised-search-11&pf_rd_t=BROWSE&pf_rd_i=12601905031";
            customExpectedName = "Lenovo";
            customExpectedPrice = 16349.00; //normal price

            var amazon = new Amazon();
            amazon = amazon.Product(customUrl);

            Assert.Equal(customExpectedName, amazon.Name);
            Assert.Equal(customExpectedPrice, amazon.Price);
            Assert.Null(amazon.Error);

        }

        [Fact]
        public void Pandora()
        {
            customUrl = "https://pandora.com.tr/kitap/shuri-the-search-for-black-panther/692816";
            customExpectedName = "Shuri: The Search for Black Panther";
            customExpectedPrice = 109.77; //normal price

            var pandora = new Pandora();
            pandora = pandora.Product(customUrl);

            Assert.Equal(customExpectedName, pandora.Name);
            Assert.Equal(customExpectedPrice, pandora.Price);
            Assert.Null(pandora.Error);
        }

        [Fact]
        public void Udemy() 
        {
            customUrl = "https://www.udemy.com/course/writing-with-flair-how-to-become-an-exceptional-writer/";
            customExpectedName = "Writing With Flair: How To Become An Exceptional Writer"; //it's coming from <meta> og:title
            customExpectedPrice = 0;

            var udemy = new Udemy();
            udemy = udemy.Product(customUrl);

            Assert.Equal(customExpectedName, udemy.Name);
            Assert.Equal(customExpectedPrice, udemy.Price);
            Assert.Null(udemy.Error);
        }
    }
}
