using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Web.Base;
using Okurdostu.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api.Me
{
    [Route("/api/me/comments")]
    public class CommentsController : SecureApiController
    {
        const int RecordPerPage = 10;
        // /api/me/comments/{SelectedPage}
        [HttpGet("{SelectedPage}")]
        public async Task<IActionResult> GetComments(int SelectedPage)
        {
            ReturnModel rm = new ReturnModel();

            if (!(SelectedPage >= 1))
            {
                rm.InternalMessage = "Page number is required";
                return Error(rm);
            }
            else
            {
                var TotalRecord = await Context.NeedComment.Where(x => x.UserId == Guid.Parse(User.Identity.GetUserId())).CountAsync();
                var TotalPage = TotalRecord / RecordPerPage;
                if (!(TotalRecord % RecordPerPage == 0))
                {
                    // mod 0 değilse, arta kalan kayıt veya kayıtlar vardır ve bunlar içinde toplam sayfa sayısına bir ekliyoruz
                    TotalPage++;
                }

                if (SelectedPage > TotalPage)
                {
                    rm.InternalMessage = "There's no page more than: " + TotalPage;
                    if (TotalPage == 0)
                    {
                        rm.Data = new { count = 0 };
                    }
                    return Error(rm);
                }


                var Comments = Context.NeedComment.AsEnumerable()
                    .Where(x => x.UserId == Guid.Parse(User.Identity.GetUserId()))
                    .OrderByDescending(x => x.CreatedOn)
                    .Select(s => new
                    {
                        s.Id,
                        s.Comment,
                        s.CreatedOn
                    })
                    .SkipLast(RecordPerPage * (SelectedPage - 1))
                    .TakeLast(RecordPerPage).ToList();

                byte ViewingRecordCount = (byte)Comments.Count();

                object Information = new
                {
                    TotalRecord,
                    TotalPage,
                    SelectedPage,
                    ViewingRecordCount
                };

                rm.Data = new { Comments, Information };
                return Succes(rm);
            }
        }
    }
}
