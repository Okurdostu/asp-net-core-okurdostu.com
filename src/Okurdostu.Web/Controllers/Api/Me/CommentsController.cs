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

        [HttpGet("{selectedPage}")]
        public async Task<IActionResult> GetComments(int SelectedPage)
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (!(SelectedPage >= 1))
            {
                jsonReturnModel.Message = "Sayfa seçilmedi";
                jsonReturnModel.InternalMessage = "Page number is required";
                return Error(jsonReturnModel);
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
                    jsonReturnModel.InternalMessage = "There isn't any page more than: " + TotalPage;
                    return Error(jsonReturnModel);
                }


                var Comments = Context.NeedComment.AsEnumerable()
                    .Where(x => x.UserId == Guid.Parse(User.Identity.GetUserId()))
                    .OrderByDescending(x => x.CreatedOn)
                    .Select(s => new
                    {
                        s.Id,
                        s.NeedId,
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

                jsonReturnModel.Data = new { Comments, Information };
                return Succes(jsonReturnModel);
            }
        }
    }
}
