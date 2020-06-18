using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "AddEducation")]
    public class AddEducationViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var Model = new EducationModel();
            await Model.ListUniversitiesAsync();
            Model.ListYears();
            return View(Model);
        }
    }
}
