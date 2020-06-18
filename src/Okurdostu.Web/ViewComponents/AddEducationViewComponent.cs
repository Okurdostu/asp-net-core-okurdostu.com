using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Okurdostu.Data;
using System.Collections.Generic;
using System.Linq;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "AddEducation")]
    public class AddEducationViewComponent : ViewComponent 
    {
        private readonly OkurdostuContext Context;
        public AddEducationViewComponent(OkurdostuContext _context) => Context = _context;

        public IViewComponentResult Invoke()
        {
            EducationModel Model = new EducationModel();

            var _Universities = new List<SelectListItem>();
            foreach (var item in  Context.University.ToList())
                _Universities.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToString() });
            Model.Universities = _Universities;

            Model.ListYears();

            return View(Model);
        }
    }
}
