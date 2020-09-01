using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Models;
using System;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditNeedDescription")]
    public class EditNeedDescriptionViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string description)
        {
            var descriptionModel = new Controllers.Api.NeedsController.DescriptionModel
            {
                Description = description
            };

            return View(descriptionModel);
        }
    }
}
