using Microsoft.AspNetCore.Mvc;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditPassword")]
    public class EditPasswordViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            Controllers.Api.MeController.PasswordModel passwordModel = new Controllers.Api.MeController.PasswordModel();
            return View(passwordModel);
        }
    }
}
