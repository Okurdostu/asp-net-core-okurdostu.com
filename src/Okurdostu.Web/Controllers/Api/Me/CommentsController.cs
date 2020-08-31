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
        [HttpGet("")]
        public ActionResult Index()
        {
            return NotFound();
        }
        const int MaxRecordPerPage = 10;
        private static int CalculateTotalPage(int TotalRecord)
        {
            return TotalRecord % MaxRecordPerPage != 0 ? (TotalRecord / MaxRecordPerPage) + 1 : TotalRecord / MaxRecordPerPage;
        }
        [HttpGet("{selectedPage}")]
        public async Task<IActionResult> GetComments([FromRoute] int selectedPage)
        {
            if (selectedPage <= 0)
            {
                return Error(null, "Page number is required");
            }
            else
            {
                var TotalRecord = await Context.NeedComment.Where(x => x.UserId == Guid.Parse(User.Identity.GetUserId())).CountAsync();
                if (TotalRecord == 0)
                {
                    return Error(null, "You have no comment", null, 404);
                }

                var TotalPage = CalculateTotalPage(TotalRecord);

                if (selectedPage > TotalPage)
                {
                    return Error(null, "There's no page more than: " + TotalPage, null, 404);
                }

                var Comments = Context.NeedComment
                    .Include(x => x.Need).ThenInclude(xNeed => xNeed.User).AsEnumerable()
                    .Where(x => x.UserId == Guid.Parse(User.Identity.GetUserId()))
                    .OrderByDescending(x => x.CreatedOn)
                    .Select(s => new
                    {
                        s.Id,
                        s.Comment,
                        s.HowLongPassed,
                        s.Need.Link
                    })
                    .SkipLast(MaxRecordPerPage * (selectedPage - 1))
                    .TakeLast(MaxRecordPerPage).ToList();

                var data = new
                {
                    TotalRecord,
                    TotalPage,
                    CurrentPage = selectedPage,
                    Count = (byte)Comments.Count(),
                    Comments
                };

                return Succes(null, data);
            }
        }
    }
}
