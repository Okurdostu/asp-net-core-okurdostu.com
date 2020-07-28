using Microsoft.AspNetCore.Mvc;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditUsername")]
    public class EditUsernameViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            Controllers.Api.MeController.UsernameModel Model = new Controllers.Api.MeController.UsernameModel();
            return View(Model);
        }
    }
}
