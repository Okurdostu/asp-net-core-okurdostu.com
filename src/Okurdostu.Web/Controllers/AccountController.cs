using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;

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
                        await Context.UserEducation.AddAsync(Education);
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
            return Redirect("/" + User.Identity.GetUsername());
        }

        [HttpPost]
        [Route("~/egitim-duzenle")]
        public async Task<IActionResult> EditEducation(EducationModel Model)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Model.EducationId);
            if (Education != null)
            {
                var AuthenticatedUser = await GetAuthenticatedUserFromDatabase();
                if (User != null && Education.UserId == AuthenticatedUser.Id)
                {
                    Education.ActivitiesSocieties = Model.ActivitiesSocieties;

                    if (Education.IsUniversityInformationsCanEditable())
                    {
                        Education.UniversityId = (short)Model.UniversityId;
                        Education.Department = Model.Department;
                    }

                    if (Model.Startyear < Model.Finishyear)
                    {
                        Education.StartYear = Model.Startyear.ToString();
                        Education.EndYear = Model.Finishyear.ToString();
                        TempData["ProfileMessage"] = "Eğitim bilgileriniz düzenlendi";
                    }
                    else
                        TempData["ProfileMessage"] = "Başlangıç yılınız, bitiriş yılınızdan büyük olmamalı" +
                            "<br />" + "Bunlar dışında ki eğitim bilgileriniz düzenlendi";

                    var result = await Context.SaveChangesAsync();
                    if (result! > 0)
                        TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                }
                else
                    TempData["ProfileMessage"] = "MC Hammer: You can't touch this";
            }
            return Redirect("/" + User.Identity.GetUsername());
        }

        [HttpPost]
        [Route("~/egitim-kaldir")]
        public async Task<IActionResult> RemoveEducation(long Id, string Username)
        {
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Id && x.User.Username == Username && !x.IsRemoved);
            if (Education != null)
            {
                var AuthenticatedUser = await GetAuthenticatedUserFromDatabase();
                if (AuthenticatedUser != null && AuthenticatedUser.Id == Education.UserId)
                {
                    var AuthenticatedUserNeedCount = Context.Need.Where(x => !x.IsRemoved && x.UserId == AuthenticatedUser.Id).Count();
                    if (Education.IsActiveEducation && AuthenticatedUserNeedCount > 0)
                        TempData["ProfileMessage"] = "İhtiyaç kampanyanız olduğu için" +
                            "<br />" +
                            "Aktif olan eğitim bilginizi silemezsiniz." +
                            "Aktif olan eğitim bilgisi, hala burada okuduğunuzu iddia ettiğiniz bir eğitim bilgisidir.";
                    else
                    {
                        Education.IsRemoved = true;
                        var result = await Context.SaveChangesAsync();
                        if (result !> 0)
                            TempData["ProfileMessage"] = "Başaramadık, neler olduğunu bilmiyoruz";
                    }
                }
                else
                    TempData["ProfileMessage"] = "MC Hammer: You can't touch this";
            }
            return Redirect("/" + User.Identity.GetUsername());
        }
    }
}
