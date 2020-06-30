using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Data.Model;
using Okurdostu.Web.Models;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "CompletedNeeds")]
    public class CompletedNeedsViewComponent : ViewComponent
    {
        
        private readonly OkurdostuContext Context;
        public CompletedNeedsViewComponent(OkurdostuContext _context) => Context = _context;
        public async Task<IViewComponentResult> InvokeAsync(long UserId)
        {

            var CompletedNeedList = await Context.Need.Where(x => !x.IsRemoved && x.UserId == UserId && x.IsCompleted).ToListAsync();
            return View(CompletedNeedList);

        }

    }
}


