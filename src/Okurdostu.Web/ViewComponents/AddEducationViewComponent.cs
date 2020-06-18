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

            //2 yerde kullanılıyor: modele taşınacak, modelden çağrılacak.
            var _Universities = new List<SelectListItem>();
            foreach (var item in await Context.University.ToListAsync())
                _Universities.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToString() });
            Model.Universities = _Universities;
            //2 yerde kullanılıyor: modele taşınacak, modelden çağrılacak.

            Model.ListYears();

            return View(Model);
        }
    }
}
