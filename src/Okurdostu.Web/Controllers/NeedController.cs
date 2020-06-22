using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using Okurdostu.Web.Models.NeedItem;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class NeedController : OkurdostuContextController
    {

        private User AuthUser;

        #region --
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
                Model.Title = Model.Title.ToLower().UppercaseFirstCharacters();

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
        #endregion



        #region needitem
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(long NeedItemId)
        {
            var item = await Context.NeedItem.Include(needitem => needitem.Need).FirstOrDefaultAsync(x => x.Id == NeedItemId && !x.Need.IsRemoved && !x.Need.IsSentForConfirmation);
            // Kampanya(need) onaylanma için yollandıysa bir item silemeyecek: onaylanma için gönderdiyse 
            //(completed ve confirmed kontrol etmeye gerek yok çünkü onaylandıysa veya bittiyse de isSentForConfirmation hep true kalacak.)
            if (item != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (AuthUser.Id == item.Need.UserId)
                {
                    item.IsRemoved = true;
                    await Context.SaveChangesAsync();
                }
                return Redirect("/" + item.Need.User.Username.ToLower() + "/ihtiyac/" + item.Need.FriendlyTitle + "/" + item.Need.Id);
            }

            ControllerContext.HttpContext.Response.StatusCode = 404;
            return null;
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(string ItemLink, long NeedId)
        {
            var Need = await Context.Need.Where(x => x.Id == NeedId
            && !x.IsRemoved
            && !x.IsSentForConfirmation
            && x.NeedItem.Where(a => a.IsRemoved != true).Count() < 3)
                .FirstOrDefaultAsync();
            // Kampanya(need) onaylanma için yollandıysa bir item ekleyemecek: onaylanma için gönderdiyse 
            //(completed ve confirmed kontrol etmeye gerek yok çünkü onaylandıysa veya bittiyse de isSentForConfirmation hep true kalacak.)
            if (Need != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (Need.UserId == AuthUser.Id)
                {
                    if (ItemLink.ToLower().Contains("udemy.com"))
                    {

                        Udemy Udemy = new Udemy();
                        Udemy = Udemy.Product(ItemLink);
                        if (Udemy.Error == null)
                        {
                            var NeedItem = new NeedItem
                            {
                                NeedId = Need.Id,
                                Link = Udemy.Link,
                                Picture = "udemy",
                                Name = Udemy.Name,
                                Price = Udemy.Price,
                                PlatformName = "Udemy",
                                IsRemoved = false,
                            };

                            await Context.AddAsync(NeedItem);
                            await Context.SaveChangesAsync();
                        }
                        else
                            TempData["Hata"] = Udemy.Error;
                    }
                    else if (ItemLink.ToLower().Contains("pandora.com.tr"))
                    {
                        if (ItemLink.ToLower().Contains("/kitap/"))
                        {

                            Pandora Pandora = new Pandora();
                            Pandora = Pandora.Product(ItemLink);
                            if (Pandora.Error == null)
                            {
                                var NeedItem = new NeedItem
                                {
                                    NeedId = Need.Id,
                                    Link = Pandora.Link,
                                    Picture = Pandora.Picture,
                                    Name = Pandora.Name,
                                    Price = Pandora.Price,
                                    PlatformName = "Pandora",
                                    IsRemoved = false,
                                    IsWrong = false
                                };

                                await Context.AddAsync(NeedItem);
                                await Context.SaveChangesAsync();
                            }
                            else
                                TempData["Hata"] = Pandora.Error;
                        }
                        else
                            TempData["MesajHata"] = "Pandora.com.tr'den sadece kitap seçebilirsiniz";
                    }
                    else if (ItemLink.ToLower().Contains("amazon.com.tr"))
                    {
                        Amazon Amazon = new Amazon();
                        Amazon = Amazon.Product(ItemLink);
                        if (Amazon.Error == null)
                        {

                            var NeedItem = new NeedItem
                            {
                                NeedId = Need.Id,
                                Link = Amazon.Link,
                                Picture = "Amazon",
                                Name = Amazon.Name,
                                Price = Amazon.Price,
                                PlatformName = "Amazon",
                                IsRemoved = false,
                            };

                            await Context.AddAsync(NeedItem);
                            await Context.SaveChangesAsync();
                        }
                        else
                            TempData["Hata"] = Amazon.Error;
                    }
                    else
                        TempData["MesajHata"] = "İhtiyacınızın linkini verirken desteklenen platformları kullanın";

                    return Redirect("/" + Need.User.Username.ToLower() + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id);
                }
            }
            ControllerContext.HttpContext.Response.StatusCode = 404;
            return null;
        }
        #endregion



        #region editneed
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTitle(NeedModel Model)
        {
            if (ModelState.ErrorCount > 1)
            {
                TempData["CreateNeedError"] = "Input error";
                return Redirect("/");
            }

            var Need = await Context.Need.FirstOrDefaultAsync(x => x.Id == Model.Id);
            if (Need != null)
            {

                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (AuthUser.Id == Need.UserId)
                {

                    if (Model.Title != Need.Title)
                    {
                        Model.Title = Model.Title.ClearSpaces();
                        Model.Title = Model.Title.ToLower().UppercaseFirstCharacters();

                        Need.Title = Model.Title;
                        Need.FriendlyTitle = Need.Title.FriendlyUrl();
                        await Context.SaveChangesAsync();
                    }

                    string link = "/" + Need.User.Username + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id;
                    return Redirect(link);
                    //catch title unique key error
                }
            }
            return null;
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDescription(NeedModel Model)
        {
            if (ModelState.ErrorCount > 1)
            {
                TempData["CreateNeedError"] = "Input error";
                return Redirect("/");
            }

            var Need = await Context.Need.FirstOrDefaultAsync(x => x.Id == Model.Id);
            if (Need != null)
            {

                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (AuthUser.Id == Need.UserId)
                {

                    if (Need.Description != Model.Description)
                    {
                        Need.Description = Model.Description;
                        await Context.SaveChangesAsync();
                    }

                    string link = "/" + Need.User.Username + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id;
                    return Redirect(link);

                }

            }
            return null;
        }
        #endregion



        #region view
        [Route("~/ihtiyac/{Id}")]
        public async Task<IActionResult> ShortUrl(long Id) // -ihtiyac/id
        {
            var Need = await Context.Need.Include(needuser=> needuser.User).FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (Need != null)
            {
                TempData["MetaRouteLink"] = "/" + Need.User.Username + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id.ToString();
                return View(Need);
            }
            else
                return Redirect("/");
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

        public PartialViewResult ViewNeedDescriptionSupporter(Need Model)
        {
            return PartialView(Model);
        }
        public PartialViewResult ViewNeedBasic(Need Model)
        {
            return PartialView(Model);
        }
        #endregion


    }
}
