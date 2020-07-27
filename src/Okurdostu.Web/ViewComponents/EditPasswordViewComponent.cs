using Microsoft.AspNetCore.Mvc;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditPassword")]
    public class EditPasswordViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            Controllers.Api.PasswordController.PasswordModel passwordModel = new Controllers.Api.PasswordController.PasswordModel();
            return View(passwordModel);
        }
    }
}
