using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditNeedDescription")]
    public class EditNeedDescriptionViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public EditNeedDescriptionViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync(long Id)
        {
            var Need = await Context.Need.FirstOrDefaultAsync(x => x.Id == Id);
            
            if (Need != null)
            {
                var NeedModel = new NeedModel
                {
                    Id = Need.Id,
                    Description = Need.Description
                };
                return View(NeedModel);
            }

            return null;
        }
    }
}
