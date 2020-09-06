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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("needcheck")]
        public async Task<JsonResult> NeedItemsPriceAndStatus(Guid needId) //checking and correcting needitem status, price.
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var Need = await Context.Need.Include(need => need.NeedItem).FirstOrDefaultAsync(x => x.Id == needId && !x.IsRemoved);
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
        [NonAction]
        public async Task<bool> IsThereAnyProblemtoCreateNeed()
        {
            var AnyActiveEducation = await Context.UserEducation.AnyAsync(x => !x.IsRemoved && x.UserId == Guid.Parse(User.Identity.GetUserId()) && x.IsActiveEducation && x.IsConfirmed);

            if (AnyActiveEducation)
            {
                Need ErrorNeed = null;
                var notRemovednotCompletedNeeds = await Context.Need.Include(need => need.User).Where(x => x.UserId == Guid.Parse(User.Identity.GetUserId()) && !x.IsRemoved && !x.IsCompleted).ToListAsync();
                if (notRemovednotCompletedNeeds.Count > 0)
                {
                    var Stage1 = notRemovednotCompletedNeeds.Where(x => x.Stage == 1).FirstOrDefault();
                    if (Stage1 != null)
                    {
                        ErrorNeed = Stage1;
                        TempData["CreateNeedError"] = "Oluşturduğunuz ama onay için gönderilmemiş bir kampanyanız var<br/>Onu tamamlayıp, onaylanması için gönderin";
                    }
                    var Stage2 = notRemovednotCompletedNeeds.Where(x => x.Stage == 2).FirstOrDefault();
                    if (Stage2 != null)
                    {
                        ErrorNeed = Stage2;
                        TempData["CreateNeedError"] = "Onaylanmamış bir kampanyanız var onun onaylanmasını bekleyin";
                    }
                    var Stage3 = notRemovednotCompletedNeeds.Where(x => x.Stage == 3).FirstOrDefault();
                    if (Stage3 != null)
                    {
                        ErrorNeed = Stage3;
                        TempData["CreateNeedError"] = "Hedefine ulaşmamış bir kampanyanız var<br/>Aynı anda iki kampanya sergiletemezsiniz";
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

        [Authorize]
        [Route("ihtiyac-olustur")]
        public async Task<IActionResult> Create()
        {
            if (await IsThereAnyProblemtoCreateNeed().ConfigureAwait(false) && TempData["CreateNeedError"] != null && TempData["CreateNeedError"].ToString() == "Active education")
            {
                TempData["ProfileMessage"] = "İhtiyaç kampanyası oluşturmak için onaylanmış bir eğitim bilgisine ihtiyacınız vardır.";
                return Redirect("/" + User.Identity.GetUsername());
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
                if (!await IsThereAnyProblemtoCreateNeed().ConfigureAwait(false))
                {
                    Model.Title = Model.Title.ClearExtraBlanks().CapitalizeFirstCharOfWords().RemoveLessGreaterSigns();
                    var Need = new Need
                    {
                        Title = Model.Title,
                        FriendlyTitle = Model.Title.FriendlyUrl(),
                        Description = Model.Description.RemoveLessGreaterSigns(),
                        UserId = Guid.Parse(User.Identity.GetUserId()),
                    };
                    try
                    {
                        await Context.Need.AddAsync(Need);
                        await Context.SaveChangesAsync();
                        Need = await Context.Need.Include(need => need.User).FirstOrDefaultAsync(x=> x.Id == Need.Id);
                        return Redirect("/" + Need.Link);
                    }
                    catch (Exception e)
                    {
                        string innerMessage = (e.InnerException != null) ? e.InnerException.Message.ToLower() : "";

                        if (innerMessage.Contains("Unique_Key_Title") || innerMessage.Contains("Unique_Key_FriendlyTitle"))
                        {
                            TempData["CreateNeedError"] = "Bu başlığı seçemezsiniz";
                        }
                        else
                        {
                            Logger.LogError("Error create need. Ex.message : {ex.message}, Ex.innermessage: {ex.inner}", e?.Message, e?.InnerException?.Message);
                            TempData["CreateNeedError"] = "Başaramadık, ne olduğunu bilmiyoruz";
                        }

                    }
                }
                else if (TempData["CreateNeedError"] != null && TempData["CreateNeedError"].ToString() == "Active education")
                {

                    TempData["ProfileMessage"] = "İhtiyaç kampanyası oluşturmak için onaylanmış bir eğitim bilgisine ihtiyacınız vardır.";
                    return Redirect("/" + User.Identity.GetUsername());

                }
            }
            else
            {
                TempData["CreateNeedError"] = "Lütfen istenen bilgileri doldurun";
            }

            return View();
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
