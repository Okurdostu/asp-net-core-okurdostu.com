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
            Controllers.Api.UsernameController.UsernameModel Model = new Controllers.Api.UsernameController.UsernameModel();
            return View(Model);
        }
    }
}
