using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data.Model;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "NeedDescriptionSupporter")]
    public class NeedDescriptionSupporterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Need need)
        {
            return View(need);
        }

    }
}
