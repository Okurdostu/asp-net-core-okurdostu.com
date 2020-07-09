using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data.Model;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using Okurdostu.Web.Models.NeedItem;
using Okurdostu.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers
{
    public class NeedController : BaseController<NeedController>
    {
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
            {
                TempData["CreateNeedError"] = "Active education";
                return true;
            }

            return false;
        }


        private User AuthUser;

        [Route("ihtiyaclar")]
        [Route("ihtiyaclar/{filtreText}")]
        [Route("ihtiyaclar/{filtreText}/{_}")]
        public async Task<IActionResult> Index(string filtreText, string _)
        {
            if (_ == "jquery")
                TempData["Jquery"] = "Yes";
            else
            {
                ViewBag.Universities = Context.University.ToList().OrderBy(x => x.Name);
                ViewData["NeedsActiveClass"] = "active";
            }

            List<Need> NeedDefaultList =
                await Context.Need
                .Include(x => x.User)
                .ThenInclude(x => x.UserEducation)
                .ThenInclude(x => x.University).Where(x => x.IsConfirmed == true && x.IsRemoved != true)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync();
            NeedDefaultList = NeedDefaultList.OrderByDescending(x => x.CompletedPercentage).ToList();
            NeedDefaultList = NeedDefaultList.OrderByDescending(x => !x.IsCompleted).ToList();

            if (!string.IsNullOrEmpty(filtreText))
            {
                if (_ != "jquery")
                    Logger.LogInformation("Searching needs with tag:{tag}, {now}", filtreText, DateTime.Now.ToString());


                var University = await Context.University.Where(x => x.FriendlyName == filtreText).FirstOrDefaultAsync();
                if (University != null) //gelen filtre bir okula uyuyorsa okula göre listele
                {
                    NeedDefaultList = NeedDefaultList.Where(x => x.User.UserEducation.Any(a => a.University.Name == University.Name && a.IsRemoved != true)).ToList();
                    TempData["ListelePageTitle"] = University.Name + " öğrencilerinin ihtiyaçları | Okurdostu";
                    ViewBag.tagUniversityName = University.Name;
                    //ViewBag.Tag = University.name;
                    ViewBag.University = University;
                }
                else // uymuyorsa, diğer containslere göre listele:
                {


                    NeedDefaultList = NeedDefaultList.Where(
                            x => x.Title.ToLower().Contains(filtreText.ToLower())
                        || x.Description.ToLower().Contains(filtreText.ToLower())
                        || x.User.Username.ToLower().Contains(filtreText.ToLower())
                        || x.User.FullName.ToLower().Contains(filtreText.ToLower())
                        ).ToList();

                    TempData["ListelePageTitle"] = filtreText + " aramasıyla ilgili öğrenci ihtiyaçları | Okurdostu";
                    ViewBag.Tag = filtreText;
                }
            }
            else // filtre yoksa komple bütün hepsini listele
            {
                TempData["ListelePageTitle"] = "Öğrencilerin ihtiyaçları | Okurdostu ";
            }
            return View(NeedDefaultList);
        }


        #region --
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task SendToConfirmation(long NeedId)
        {
            var Need = await Context.Need.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == NeedId);

            if (Need != null)
            {

                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (Need.UserId == AuthUser.Id)
                {
                    var UnRemovedItems = await Context.NeedItem.Where(x => x.NeedId == Need.Id && !x.IsRemoved).ToListAsync();
                    if (UnRemovedItems.Count() > 0 && UnRemovedItems.Count() <= 3)
                    {
                        decimal TotalCharge = 0;

                        foreach (var item in UnRemovedItems)
                            TotalCharge += item.Price;

                        Need.TotalCollectedMoney = 0;
                        Need.TotalCharge = TotalCharge;
                        Need.IsSentForConfirmation = true;

                        await Context.SaveChangesAsync();

                    }
                    else
                        TempData["NeedMessage"] = "Kampanyanızı onaya yollamak için en az bir, en fazla üç hedef belirlemelisiniz";

                    Response.Redirect("/" + Need.User.Username.ToLower() + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id);
                }
            }
        }

        [Authorize]
        [Route("ihtiyac-olustur")]
        public async Task<IActionResult> Create()
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (await IsThereAnyProblemtoCreateNeed() && TempData["CreateNeedError"] != null && TempData["CreateNeedError"].ToString() == "Active education")
            {
                TempData["ProfileMessage"] = "İhtiyaç kampanyası oluşturmak için onaylanmış bir eğitim bilgisine ihtiyacınız vardır.";
                return Redirect("/" + AuthUser.Username);
            }
            return View();
        }


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        [Route("ihtiyac-olustur")]
        public async Task<IActionResult> Create(NeedModel Model)
        {
            if (ModelState.IsValid)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

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
                        {
                            TempData["CreateNeedError"] = "Bu başlığı seçemezsiniz";
                        }
                        else
                        {
                            Logger.LogError("Error create need. Ex.message : {ex.message}, Ex.innermessage: {ex.inner}", e.Message, e.InnerException.Message);
                            TempData["CreateNeedError"] = "Başaramadık, ne olduğunu bilmiyoruz";
                        }

                    }
                }
                else if (TempData["CreateNeedError"] != null && TempData["CreateNeedError"].ToString() == "Active education")
                {

                    TempData["ProfileMessage"] = "İhtiyaç kampanyası oluşturmak için onaylanmış bir eğitim bilgisine ihtiyacınız vardır.";
                    return Redirect("/" + AuthUser.Username);

                }
            }
            else
            {
                TempData["CreateNeedError"] = "Lütfen istenen bilgileri doldurun";
            }

            return View();
        }
        #endregion


        #region needitemremoveadd
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task RemoveItem(long NeedItemId)
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
                    Response.Redirect("/" + item.Need.User.Username.ToLower() + "/ihtiyac/" + item.Need.FriendlyTitle + "/" + item.Need.Id);
                }
            }
        }


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task AddItem(string ItemLink, long NeedId)
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
                            Need.TotalCharge += NeedItem.Price;
                            await Context.SaveChangesAsync();
                        }
                        else
                        {
                            Logger.LogError("Udemy Error:{error}, Link:{link}", Udemy.Error, ItemLink);
                            TempData["NeedMessage"] = Udemy.Error;
                        }
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
                                Need.TotalCharge += NeedItem.Price;
                                await Context.SaveChangesAsync();
                            }
                            else
                            {
                                Logger.LogError("Pandora Error:{error}, Link:{link}", Pandora.Error, ItemLink);
                                TempData["NeedMessage"] = Pandora.Error;
                            }
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
                            Need.TotalCharge += NeedItem.Price;
                            await Context.SaveChangesAsync();
                        }
                        else
                        {
                            Logger.LogError("Amazon Error:{error}, Link:{link}", Amazon.Error, ItemLink);
                            TempData["NeedMessage"] = Amazon.Error;
                        }
                    }
                    else
                        TempData["MesajHata"] = "İhtiyacınızın linkini verirken desteklenen platformları kullanın";

                    Response.Redirect("/" + Need.User.Username.ToLower() + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id);
                }
            }
        }
        #endregion


        #region editneedtitledescription
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task EditTitle(NeedModel Model)
        {

            var Need = await Context.Need.FirstOrDefaultAsync(x => x.Id == Model.Id);
            if (Need != null)
            {

                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (AuthUser.Id == Need.UserId)
                {
                    if (ModelState.ErrorCount < 2)
                    {
                        if (Model.Title != Need.Title)
                        {
                            Model.Title = Model.Title.ClearSpaces();
                            Model.Title = Model.Title.ToLower().UppercaseFirstCharacters();

                            Need.Title = Model.Title;
                            Need.FriendlyTitle = Need.Title.FriendlyUrl();
                            try
                            {
                                await Context.SaveChangesAsync();
                                TempData["NeedMessage"] = "Başlık düzenlendi";
                            }
                            catch (Exception e)
                            {
                                if (e.InnerException.Message.Contains("Unique_Key_Title"))
                                    TempData["NeedMessage"] = "Bu başlığı seçemezsiniz";
                            }
                        }
                    }
                    else
                    {
                        TempData["NeedMessage"] = "Başlık boş olamaz.<br />" +
                            "En fazla 75 karakter ";
                    }
                    string link = "/" + Need.User.Username + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id;
                    Response.Redirect(link);
                }
            }
        }


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task EditDescription(NeedModel Model)
        {

            var Need = await Context.Need.FirstOrDefaultAsync(x => x.Id == Model.Id);
            if (Need != null)
            {

                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (AuthUser.Id == Need.UserId)
                {
                    if (ModelState.ErrorCount < 2)
                    {
                        if (Need.Description != Model.Description)
                        {
                            Need.Description = Model.Description;
                            await Context.SaveChangesAsync();
                            TempData["NeedMessage"] = "Açıklama düzenlendi";
                        }
                    }
                    else
                    {
                        TempData["NeedMessage"] = "Açıklama boş olamaz.<br />" +
                            "En az: 100, en fazla 10 bin karakter";
                    }

                    string link = "/" + Need.User.Username + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id;
                    Response.Redirect(link);
                }

            }
        }
        #endregion


        #region view
        [Route("ihtiyac/{Id}")]
        public async Task<IActionResult> ShortUrl(long Id)
        {
            var Need = await Context.Need.Include(needuser => needuser.User).FirstOrDefaultAsync(x => x.Id == Id && !x.IsRemoved);

            if (Need != null)
            {
                TempData["MetaRouteLink"] = "/" + Need.User.Username + "/ihtiyac/" + Need.FriendlyTitle + "/" + Need.Id.ToString();
                return View(Need);
            }
            else
                return Redirect("/ihtiyaclar");
        }


        [Route("{username}/ihtiyac/{friendlytitle}/{id}")]
        public async Task<IActionResult> ViewNeed(string username, string friendlytitle, long id)
        {
            ViewData["NeedsActiveClass"] = "active";
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

            return NotFound();
        }
        #endregion
    }
}
