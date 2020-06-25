using Microsoft.AspNetCore.Mvc;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditPassword")]
    public class EditPasswordViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            ProfileModel Model = new ProfileModel();
            return View(Model);
        }
    }
}
