using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using Okurdostu.Web.Models.NeedItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

                    TempData["CausingErrorNeedLink"] = ErrorNeed.Link;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("needcheck")]
        public async Task<JsonResult> NeedItemsPriceAndStatus(Guid needId) //checking and correcting needitem status, price.
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var Need = await Context.Need.Include(need => need.NeedItem).FirstOrDefaultAsync(x => x.Id == needId && !x.IsRemoved && !x.IsCompleted && x.IsSentForConfirmation);
            bool IsPageNeedRefresh = false;
            if (Need != null && Need.ShouldBeCheck)
            {
                Need.IsWrong = false; //need önceden hatalı olarak işaretlenmiş olabilir her ihtimale karşı hatasının gitmiş olabileceğini var sayarak, 
                                      //false'lıyoruz eğer ki hala hatalıysa zaten tekrar hatalı olarak işaretlenecektir.
                var NeedItems = Need.NeedItem.Where(x => !x.IsRemoved).ToList();

                decimal TotalCharge = 0;
                foreach (var item in NeedItems)
                {
                    item.IsWrong = false;   //item önceden hatalı olarak işaretlenmiş olabilir her ihtimale karşı hatasının gitmiş olabileceğini var sayarak, 
                                            //false'lıyoruz eğer ki hala hatalıysa zaten tekrar hatalı olarak işaretlenecektir.
                    if (item.PlatformName == "Amazon")
                    {
                        var Amazon = new Amazon();
                        Amazon = Amazon.Product(item.Link);

                        if (Amazon.Error == null) // herhangi bir hata yoksa
                        {
                            if (item.Price != (decimal)Amazon.Price) //ürünün amazon sisteminde ki fiyatı ile veritabanında ki fiyatı eşleşmiyorsa
                            {
                                IsPageNeedRefresh = true;
                                item.Price = (decimal)Amazon.Price; //item'a yeni fiyatını al.
                            }
                        }
                        else // hatalı ise ürünü ve kampanyayı hatalı olarak işaretliyoruz, hatalı işaretlenen durumlar kontrol edilmesi için panelde görüntülenecek.
                        {
                            item.IsWrong = true;
                            Need.IsWrong = true;
                        }
                    }
                    else if (item.PlatformName == "Pandora")
                    {
                        var Pandora = new Pandora();
                        Pandora = Pandora.Product(item.Link);

                        if (Pandora.Error == null)
                        {
                            if (item.Price != (decimal)Pandora.Price)
                            {
                                IsPageNeedRefresh = true;
                                item.Price = (decimal)Pandora.Price;
                            }
                        }
                        else
                        {
                            item.IsWrong = true;
                            Need.IsWrong = true;
                        }
                    }
                    else
                    {
                        var Udemy = new Udemy();
                        Udemy = Udemy.Product(item.Link);
                        //udemy fiyatı ile ilgili sorun var. sabit bir değer atılıyor, fiyat değişikliğini kontrol etmeye gerek yok.
                        if (Udemy.Error != null)
                        {
                            item.IsWrong = true;
                            Need.IsWrong = true;
                        }
                    }

                    TotalCharge += item.Price; //fiyat değişikliği kontrol edilip güncellenmesi gerekiyorsa güncellendikten sonra ki fiyatını almalıyız ki totalcharge'i da güncelleyebilelim.
                }

                if (!Need.IsWrong && Need.TotalCharge != TotalCharge) //kampanya hatalı olarak işaretlendiyse totalcharge güncellemesi yaptırmıyoruz.
                {
                    IsPageNeedRefresh = true;
                    Need.TotalCharge = TotalCharge;
                }
                Need.LastCheckOn = DateTime.Now;

                await Context.SaveChangesAsync(); //değiştirilen bütün verileri sadece bir kere kaydediyoruz.
            }
            return Json(new { IsPageNeedRefresh });
        }


        private User AuthUser;

        [Route("ihtiyaclar")]
        [Route("ihtiyaclar/{filtreText}")]
        [Route("ihtiyaclar/{filtreText}/{_}")]
        public async Task<IActionResult> Index(string filtreText, string _)
        {
            if (_ == "jquery")
            {
                TempData["Jquery"] = "Yes";
            }
            else
            {
                ViewBag.Universities = Context.University.ToList().OrderBy(x => x.Name);
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
                var University = await Context.University.Where(x => x.FriendlyName == filtreText).FirstOrDefaultAsync();
                if (University != null) //gelen filtre bir okula uyuyorsa okula göre listele
                {
                    NeedDefaultList = NeedDefaultList.Where(x => x.User.UserEducation.Any(a => a.University.Name == University.Name && a.IsRemoved != true)).ToList();
                    TempData["ListelePageTitle"] = University.Name + " öğrencilerinin ihtiyaçları | Okurdostu";
                    ViewBag.tagUniversityName = University.Name;
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
        public async Task SendToConfirmation(Guid NeedId)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

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
                        {
                            TotalCharge += item.Price;
                        }

                        Need.TotalCharge = TotalCharge;
                        Need.IsSentForConfirmation = true;

                        await Context.SaveChangesAsync();

                    }
                    else
                    {
                        TempData["NeedMessage"] = "Kampanyanızı onaya yollamak için en az bir, en fazla üç hedef belirlemelisiniz";
                    }

                    Response.Redirect("/" + Need.Link);
                }
            }
        }

        [Authorize]
        [Route("ihtiyac-olustur")]
        public async Task<IActionResult> Create()
        {
            AuthUser = await GetAuthenticatedUserFromDatabaseAsync();

            if (await IsThereAnyProblemtoCreateNeed().ConfigureAwait(false) && TempData["CreateNeedError"] != null && TempData["CreateNeedError"].ToString() == "Active education")
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

                if (!await IsThereAnyProblemtoCreateNeed().ConfigureAwait(false))
                {
                    Model.Title = Model.Title.ClearBlanks();
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
                            return Redirect("/" + Need.Link);
                        }

                    }
                    catch (Exception e)
                    {

                        if (e.InnerException.Message != null && e.InnerException.Message.Contains("Unique_Key_Title") || e.InnerException.Message.Contains("Unique_Key_FriendlyTitle"))
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
        public async Task RemoveItem(Guid NeedItemId)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var item = await Context.NeedItem.Include(needitem => needitem.Need).FirstOrDefaultAsync(x => x.Id == NeedItemId
            && !x.Need.IsRemoved
            && !x.Need.IsSentForConfirmation);
            // Kampanya(need) onaylanma için yollandıysa bir item silemeyecek: onaylanma için gönderdiyse 
            //(completed ve confirmed kontrol etmeye gerek yok çünkü onaylandıysa veya bittiyse de isSentForConfirmation hep true kalacak.)
            if (item != null)
            {
                AuthUser = await GetAuthenticatedUserFromDatabaseAsync();
                if (AuthUser.Id == item.Need.UserId) //item(ihtiyaç)'ı silmeye çalıştığı kampanya Authenticated olmuş üzere aitse..
                {
                    item.Need.TotalCharge -= item.Price;
                    Context.Remove(item);
                    await Context.SaveChangesAsync();
                    Response.Redirect("/" + item.Need.Link);
                }
            }
        }

        #endregion

        #region view
        [Route("{username}/ihtiyac/{friendlytitle}")]
        public async Task<IActionResult> ViewNeed(string username, string friendlytitle)
        {
            var Need = await Context.Need
                .Include(need => need.User)
                        .ThenInclude(needuser => needuser.UserEducation)
                            .ThenInclude(x => x.University)
                .Include(need => need.NeedItem)
                .FirstOrDefaultAsync(x => x.User.Username == username && x.FriendlyTitle == friendlytitle && !x.IsRemoved);

            if (Need != null)
            {
                TempData["activeihtiyac"] = "active";
                return View(Need);
            }
            else
            {
                //böyle bir ihtiyaç kampanyasının olmadığını söyleyen bir sayfa yapılacak
                return Redirect("/ihtiyaclar");
            }
        }
        #endregion
    }
}
