using Microsoft.AspNetCore.Mvc;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditPassword")]
    public class EditPasswordViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            Controllers.Api.AccountController.PasswordModel passwordModel = new Controllers.Api.AccountController.PasswordModel();
            return View(passwordModel);
        }
    }
}
