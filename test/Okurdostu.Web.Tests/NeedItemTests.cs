using Okurdostu.Web.Models.NeedItem;
using Xunit;

namespace Okurdostu.Web.Tests
{
    public class NeedItemTests
    {
        //product page url amazon, pandora or udemy
        private string customUrl = "https://www.amazon.com.tr/Lenovo-30BFS3W400-W-2135-GTX1080-512SSD/dp/B08BG6W56C?ref_=s9_apbd_orecs_hd_bw_bDkqJAl&pf_rd_r=7G8HYQ84V5WC1RQSETBV&pf_rd_p=f9a1563d-4e14-5798-a39a-e461d59b63a6&pf_rd_s=merchandised-search-11&pf_rd_t=BROWSE&pf_rd_i=12601905031";
        //expected product brand name is for amazon, product name is for pandora, course name is for udemy
        private string customExpectedName = "Lenovo";
        //expected discounted course price, discount pandora book price and discount amazon product price.
        private double customExpectedPrice = 16349.00;

        [Fact]
        public void AmazonBrandName()
        {
            var amazon = new Amazon();
            amazon = amazon.Product(customUrl);

            Assert.Equal(customExpectedName, amazon.Name);
        }

        [Fact]
        public void AmazonPrice()
        {
            var amazon = new Amazon();
            amazon = amazon.Product(customUrl);

            Assert.Equal(customExpectedPrice, (double)amazon.Price);
        }

        [Fact]
        public void PandoraProductName()
        {
            var pandora = new Pandora();
            pandora = pandora.Product(customUrl);

            Assert.Equal(customExpectedName, pandora.Name);
        }

        [Fact]
        public void PandoraPrice()
        {
            var pandora = new Pandora();
            pandora = pandora.Product(customUrl);

            Assert.Equal(customExpectedPrice, (double)pandora.Price);
        }

        [Fact]
        public void UdemyCourseName() //it's coming from <meta> og:title
        {
            var udemy = new Udemy();
            udemy = udemy.Product(customUrl);

            Assert.Equal(customExpectedName, udemy.Name);
        }

        [Fact]
        public void UdemyPrice()
        {
            var udemy = new Udemy();
            udemy = udemy.Product(customUrl);

            Assert.Equal(customExpectedPrice, (double)udemy.Price);
        }
    }
}
