using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Models;
using System;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditNeedTitle")]
    public class EditNeedTitleViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string Title)
        {
            var titleModel = new Controllers.Api.Me.NeedsController.TitleModel
            {
                Title = Title
            };

            return View(titleModel);
        }
    }
}
