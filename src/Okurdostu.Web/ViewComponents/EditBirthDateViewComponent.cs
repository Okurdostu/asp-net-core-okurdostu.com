using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditBirthDate")]
    public class EditBirthDate : ViewComponent
    {
        public IViewComponentResult Invoke(int Year = 0, int Month = 0, int Day = 0, bool AreBDMonthDayPublic = false, bool IsBDYearPublic = false)
        {
            Controllers.Api.MeController.BirthdateModel birthdateModel = new Controllers.Api.MeController.BirthdateModel
            {
                AreBDMonthDayPublic = AreBDMonthDayPublic,
                Day = Day,
                IsBDYearPublic = IsBDYearPublic,
                Month = Month,
                Year = Year
            };
            return View(birthdateModel);
        }
    }
}
