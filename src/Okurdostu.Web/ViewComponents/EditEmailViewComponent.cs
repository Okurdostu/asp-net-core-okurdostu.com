using Microsoft.AspNetCore.Mvc;
using Okurdostu.Web.Models;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "EditEmail")]
    public class EditEmail : ViewComponent
    {

        public IViewComponentResult Invoke(string Email)
        {
            ProfileModel Model = new ProfileModel
            {
                Email = Email
            };
            return View(Model);
        }
    }
}
