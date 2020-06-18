using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Base;

namespace Okurdostu.Web.Controllers
{
    [Authorize]
    public class AccountController : OkurdostuContextController
    {
        [HttpPost]
        [Route("~/egitim-ekle")]
        public async Task<IActionResult> AddEducation(EducationModel Model)
        {
            var University = await Context.University.FirstOrDefaultAsync(x => x.Id == Model.UniversityId);
            if (University != null)
            {
                if (Model.Startyear < Model.Finishyear)
                {
                    var User = await GetAuthenticatedUserFromDatabase();
                    if (User != null)
                    {
                        var Education = new UserEducation
                        {
                            UserId = User.Id,
                            UniversityId = University.Id,
                            Department = Model.Department,
                            StartYear = Model.Startyear.ToString(),
                            EndYear = Model.Finishyear.ToString(),
                            ActivitiesSocieties = Model.ActivitiesSocieties
                        };
                        await Context.AddAsync(Education);
                        var result = await Context.SaveChangesAsync();
                        if (result > 0)
                            TempData["ProfileMessage"] = "Eğitim bilginiz eklendi<br />Onaylanması için belge yollamayı unutmayın.";
                        else
                            TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                    }
                }
                else
                    TempData["ProfileMessage"] = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı";

            }
            TempData["ProfileMessage"] = "Böyle bir üniversite yok";
            return Redirect("/" + User.Identity.GetUsername());
        }

    }
}
