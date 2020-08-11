using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditBirthDate")]
    public class EditBirthDate : ViewComponent
    {
        public IViewComponentResult Invoke(int Year = 0, int Month = 0, int Day = 0, sbyte BDSecretLevel = 0)
        {
            Controllers.Api.MeController.BirthdateModel birthdateModel = new Controllers.Api.MeController.BirthdateModel
            {
                Day = Day,
                Month = Month,
                Year = Year,
                BDSecretLevel = BDSecretLevel
            };

            return View(birthdateModel);
        }
    }
}
