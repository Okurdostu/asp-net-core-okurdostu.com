using Microsoft.AspNetCore.Mvc;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "UniversityStudents")]
    public class UniversityStudentsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string friendlyname)
        {
            UniversityStudentsModel USM = new UniversityStudentsModel();
            return View(await USM.Students(friendlyname));
        }
    }
}
