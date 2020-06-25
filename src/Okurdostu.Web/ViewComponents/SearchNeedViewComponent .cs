using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "SearchNeed")]
    public class SearchNeedViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public SearchNeedViewComponent(OkurdostuContext _context) => Context = _context;

        public async Task<IViewComponentResult> InvokeAsync(string q)
        {
            var searchResult = await Context.Need.Include(x=> x.User).Where(x =>
            x.IsConfirmed && !x.IsRemoved && x.Title.ToLower().Contains(q.ToLower())
            ).ToListAsync();

            return View(searchResult);
        }
    }
}
