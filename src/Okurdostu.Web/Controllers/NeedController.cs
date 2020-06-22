using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class NeedController : OkurdostuContextController
    {

        private User AuthUser;


        [NonAction]
        public async Task<bool> IsThereAnyProblemtoCreateNeed()
        {
            var AuthUserAnyActiveEducation = await Context.UserEducation.FirstOrDefaultAsync(x => !x.IsRemoved && x.UserId == AuthUser.Id && x.IsActiveEducation && x.IsConfirmed);

            if (AuthUserAnyActiveEducation != null)
            {
                Need ErrorNeed = null;
                var UserNotRemovedCompletedNeeds = await Context.Need.Where(x => x.UserId == AuthUser.Id && !x.IsRemoved && !x.IsCompleted).ToListAsync();
                if (UserNotRemovedCompletedNeeds.Count > 0) //User'in önceden oluşturduğu ve silmediği bir kampanya varsa
                {
                    if (UserNotRemovedCompletedNeeds.Where(x => x.IsConfirmed == false && x.IsSentForConfirmation == false).FirstOrDefault() != null)
                    {
                        //Onaylama için gönderilmemiş bir kampanya varsa
                        ErrorNeed = UserNotRemovedCompletedNeeds.Where(x => x.IsConfirmed == false && x.IsSentForConfirmation == false).FirstOrDefault();
                        TempData["CreateNeedError"] = "Oluşturduğunuz ama onay için gönderilmemiş bir kampanyanız var <br/> Onu tamamlayıp, onaylanması için gönderin";
                    }
                    else if (UserNotRemovedCompletedNeeds.Where(x => x.IsConfirmed == false && x.IsSentForConfirmation == true).FirstOrDefault() != null)
                    {
                        //Onaylanmamış fakat onaylanması için gönderilmiş bir kampanya varsa
                        ErrorNeed = UserNotRemovedCompletedNeeds.Where(x => x.IsConfirmed == false && x.IsSentForConfirmation == true).FirstOrDefault();
                        TempData["CreateNeedError"] = "Onaylanmamış bir kampanyanız var onun onaylanmasını bekleyin";
                    }
                    else if (UserNotRemovedCompletedNeeds.Where(x => x.IsConfirmed == true && x.IsCompleted == false).FirstOrDefault() != null)
                    {
                        //Onaylanmış fakat tamamlanmamış bir kampanya varsa
                        ErrorNeed = UserNotRemovedCompletedNeeds.Where(x => x.IsConfirmed == true && x.IsCompleted == false).FirstOrDefault();
                        TempData["CreateNeedError"] = "Hedefine ulaşmamış bir kampanyanız var <br/> Aynı anda iki kampanya sergiletemezsiniz";
                    }

                    TempData["CausingErrorNeedLink"] = "/" + ErrorNeed.User.Username + "/ihtiyac/" + ErrorNeed.FriendlyTitle + "/" + ErrorNeed.Id;
                    return true;
                }
            }
            else
                TempData["CreateNeedError"] = "Active education";

            return false;
        }


        [Route("~/ihtiyaclar")]
        public async Task<IActionResult> Index()
        {
            return View();
        }


        [Authorize]
        [Route("~/ihtiyac-olustur")]
        public async Task<IActionResult> Create()
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (await IsThereAnyProblemtoCreateNeed())
            {

                if (TempData["CreateNeedError"].ToString() == "Active education")
                {
                    TempData["ProfileMessage"] = "İhtiyaç kampanyası oluşturmak için onaylanmış bir eğitim bilgisine ihtiyacınız vardır.";
                    return Redirect("/" + AuthUser.Username);
                }

            }
            return View();
        }


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/ihtiyac-olustur")]
        public async Task<IActionResult> Create(NeedModel Model)
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
            if (ModelState.ErrorCount > 1)
            {
                TempData["CreateNeedError"] = "Input error";
                return View();
            }

            if (!await IsThereAnyProblemtoCreateNeed())
            {
                Model.Title = Model.Title.ClearSpaces();
                Model.Title = Model.Title.UppercaseFirstCharacters();

                var Need = new Need
                {
                    Title = Model.Title,
                    FriendlyTitle = Model.Title.FriendlyUrl(),
                    Description = Model.Description,
                    UserId = AuthUser.Id,
                };

                try
                {

                    await Context.Need.AddAsync(Need);
                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {

                        string link = "/" + Need.User.Username + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id;
                        return Redirect(link);

                    }

                }
                catch (Exception e)
                {

                    if (e.InnerException.Message.Contains("Unique_Key_Title"))
                        TempData["CreateNeedError"] = "Bu başlığı seçemezsiniz";
                    else
                        TempData["CreateNeedError"] = "Başaramadık, ne olduğunu bilmiyoruz";

                }
            }
            else if (TempData["CreateNeedError"].ToString() == "Active education")
            {

                TempData["ProfileMessage"] = "İhtiyaç kampanyası oluşturmak için onaylanmış bir eğitim bilgisine ihtiyacınız vardır.";
                return Redirect("/" + AuthUser.Username);

            }

            return View();
        }

        [Route("~/{username}/ihtiyac/{friendlytitle}/{id}")]
        public async Task<IActionResult> ViewNeed(string username, string friendlytitle, long id)
        {
            var Need = await Context.Need
                .Include(need => need.User)
                        .ThenInclude(needuser => needuser.UserEducation)
                            .ThenInclude(x => x.University)
                .Include(need => need.NeedItem)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsRemoved);

            if (Need != null)
            {

                TempData["activeihtiyac"] = "active";
                if (Need.User.Username.ToLower() != username.ToLower() || Need.FriendlyTitle.ToLower() != friendlytitle.ToLower())
                {
                    return Redirect("/" + Need.User.Username.ToLower() + "/ihtiyac/" + Need.FriendlyTitle.ToLower() + "/" + Need.Id);
                }

                return View(Need);

            }

            //404
            return null;
        }

        public PartialViewResult ViewNeedItem(Need Model)
        {
            return PartialView(Model);
        }


    }
}
