using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "OpenNeeds")]
    public class OpenNeedsViewComponent : ViewComponent
    {
        
        private readonly OkurdostuContext Context;
        public OpenNeedsViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync(long UserId)
        {

            var OpenNeedList = await Context.Need.Where(x => !x.IsRemoved && x.IsConfirmed && x.UserId == UserId && !x.IsCompleted).ToListAsync();
            return View(OpenNeedList);

        }

    }
}



