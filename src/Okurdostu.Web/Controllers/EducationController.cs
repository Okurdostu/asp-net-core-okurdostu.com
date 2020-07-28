using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    [Authorize]
    public class EducationController : BaseController<EducationController>
    {
        public async Task<IActionResult> EditView([FromQuery] EducationModel educationModel)
        {
            educationModel.Universities = await Context.University.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() }).ToListAsync();
            educationModel.ListYears();
            return View(educationModel);
        }
    }
}
