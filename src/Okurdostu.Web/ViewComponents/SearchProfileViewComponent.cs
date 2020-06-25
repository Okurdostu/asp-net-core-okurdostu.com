using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "SearchProfile")]
    public class SearchProfileViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public SearchProfileViewComponent(OkurdostuContext _context) => Context = _context;

        public async Task<IViewComponentResult> InvokeAsync(string q)
        {
            var searchResult = await Context.User.Where(x =>
            x.Username.ToLower().Contains(q.ToLower()) ||
            x.FullName.ToLower().Contains(q.ToLower()) ||
            x.Biography.ToLower().Contains(q.ToLower()
            )).ToListAsync();
            return View(searchResult);
        }
    }
}
