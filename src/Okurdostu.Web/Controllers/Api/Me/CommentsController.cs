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
            var TotalPage = TotalRecord / MaxRecordPerPage;
            if (TotalRecord % MaxRecordPerPage != 0) TotalPage++;

            return TotalPage;
        }

        [HttpGet("{selectedPage}")]
        public async Task<IActionResult> GetComments([FromRoute] int selectedPage)
        {
            ReturnModel rm = new ReturnModel();
            if (selectedPage <= 0)
            {
                rm.InternalMessage = "Page number is required";
                return Error(rm);
            }
            else
            {
                var TotalRecord = await Context.NeedComment.Where(x => x.UserId == Guid.Parse(User.Identity.GetUserId())).CountAsync();
                if (TotalRecord == 0)
                {
                    rm.Message = "You have no comment";
                    rm.Code = 404;
                    return Error(rm);
                }

                var TotalPage = CalculateTotalPage(TotalRecord);

                if (selectedPage > TotalPage)
                {
                    rm.InternalMessage = "There's no page more than: " + TotalPage;
                    rm.Code = 404;
                    return Error(rm);
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

                object Information = new
                {
                    TotalRecord,
                    TotalPage,
                    CurrentPage = selectedPage,
                    Count = (byte)Comments.Count()
                };

                rm.Data = new { Information, Comments };
                return Succes(rm);
            }
        }
    }
}
