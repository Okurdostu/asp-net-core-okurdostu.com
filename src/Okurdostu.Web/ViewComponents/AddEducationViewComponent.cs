using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "AddEducation")]
    public class AddEducationViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public AddEducationViewComponent(OkurdostuContext _context)
        {
            Context = _context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var Model = new EducationModel();
            Model.ListYears();
            Model.Universities = await Context.University.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() }).ToListAsync();
            return View(Model);
        }
    }
}
