using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "AddEducation")]
    public class AddEducationViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public AddEducationViewComponent(OkurdostuContext _context) => Context = _context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var Model = new EducationModel();
            await Model.ListUniversitiesAsync();
            Model.ListYears();
            return View(Model);
        }
    }
}
