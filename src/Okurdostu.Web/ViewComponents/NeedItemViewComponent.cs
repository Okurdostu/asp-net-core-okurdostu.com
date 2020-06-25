using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data.Model;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "NeedItem")]
    public class NeedItemViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Need need)
        {
            return View(need);
        }

    }
}
