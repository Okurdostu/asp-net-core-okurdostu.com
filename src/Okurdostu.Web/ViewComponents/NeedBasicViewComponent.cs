using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data.Model;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "NeedBasic")]
    public class NeedBasicViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Need need)
        {
            return View(need);
        }

    }
}
