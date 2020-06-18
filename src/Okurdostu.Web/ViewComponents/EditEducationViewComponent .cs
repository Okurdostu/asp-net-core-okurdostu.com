using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditEducation")]
    public class EditEducationViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public EditEducationViewComponent(OkurdostuContext _context) => Context = _context;

        public async Task<IViewComponentResult> InvokeAsync(long EducationdId)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == EducationdId);
            if (Education != null)
            {
                var Model = new EducationModel
                {
                    EducationId = EducationdId,
                    ActivitiesSocieties = Education.ActivitiesSocieties,
                    Startyear = int.Parse(Education.StartYear),
                    Finishyear = int.Parse(Education.EndYear),
                };
                Model.ListYears();
                if (Education.IsUniversityInformationsCanEditable())
                {
                    Model.UniversityId = Education.UniversityId;
                    Model.Department = Education.Department;

                    //2 yerde kullanılıyor: modele taşınacak, modelden çağrılacak.
                    var _Universities = new List<SelectListItem>();
                    foreach (var item in Context.University.ToList())
                        _Universities.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToString() });
                    Model.Universities = _Universities;
                    //2 yerde kullanılıyor: modele taşınacak, modelden çağrılacak.
                }
                return View(Model);
            }
            return null;
        }
    }
}
