using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "SearchUniversity")]
    public class SearchUniversityViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public SearchUniversityViewComponent(OkurdostuContext _context) => Context = _context;

        public async Task<IViewComponentResult> InvokeAsync(string q)
        {
            var searchResult = await Context.University.Where(x => x.Name.ToLower().Contains(q.ToLower())).OrderByDescending(x => x.Name).ToListAsync();
            return View(searchResult);
        }
    }
}
