using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;
using Okurdostu.Web.Models.NeedItem;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    public class NeedsController : SecureApiController
    {
        [HttpGet("")]
        public ActionResult Index()
        {
            return NotFound();
        }

        [NonAction]
        public async Task AddNeedItem(Guid needId, string link, string name, double price, string picture, string platformName)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var NeedItem = new NeedItem
            {
                NeedId = needId,
                Link = link,
                Name = name,
                Price = (decimal)price,
                Picture = picture,
                PlatformName = platformName,
            };

            await Context.AddAsync(NeedItem);
            NeedItem.Need.TotalCharge += NeedItem.Price;
            await Context.SaveChangesAsync();
        }

        [ValidateAntiForgeryToken]
        [HttpPost("item")]
        public async Task<IActionResult> AddItem(string itemLink, Guid needId)
        {
            sbyte maxItemCount = 3;
            var AuthenticatedUserId = Guid.Parse(User.Identity.GetUserId());

            var Need = await Context.Need.Where(x => x.Id == needId
            && x.UserId == AuthenticatedUserId
            && !x.IsRemoved
            && !x.IsSentForConfirmation
            && x.NeedItem.Count() < maxItemCount).FirstOrDefaultAsync();

            if (Need != null)
            {
                if (itemLink.Contains("udemy.com"))
                {
                    Udemy Udemy = new Udemy();
                    Udemy = Udemy.Product(itemLink);
                    if (Udemy.Error == null)
                    {
                        await AddNeedItem(Need.Id, Udemy.Link, Udemy.Name, Udemy.Price, "/image/udemy.png", "Udemy").ConfigureAwait(false);
                        return Succes(null, Udemy, 201);
                    }
                    else
                    {
                        Logger.LogError("Udemy Error:{error}, Link:{link}", Udemy.Error, itemLink);
                        return Error(Udemy.Error);
                    }
                }
                else if (itemLink.Contains("amazon.com.tr"))
                {
                    Amazon Amazon = new Amazon();
                    Amazon = Amazon.Product(itemLink);
                    if (Amazon.Error == null)
                    {
                        await AddNeedItem(Need.Id, Amazon.Link, Amazon.Name, Amazon.Price, "/image/amazon.png", "Amazon").ConfigureAwait(false);
                        return Succes(null, Amazon, 201);
                    }
                    else
                    {
                        Logger.LogError("Amazon Error:{error}, Link:{link}", Amazon.Error, itemLink);
                        return Error(Amazon.Error);
                    }
                }
                else if (itemLink.Contains("pandora.com.tr"))
                {
                    if (itemLink.ToLower().Contains("/kitap/"))
                    {
                        Pandora Pandora = new Pandora();
                        Pandora = Pandora.Product(itemLink);
                        if (Pandora.Error == null)
                        {
                            await AddNeedItem(Need.Id, Pandora.Link, Pandora.Name, Pandora.Price, Pandora.Picture, "Pandora").ConfigureAwait(false);
                            return Succes(null, Pandora, 201);
                        }
                        else
                        {
                            Logger.LogError("Pandora Error:{error}, Link:{link}", Pandora.Error, itemLink);
                            return Error(Pandora.Error);
                        }
                    }
                    
                    return Error("Pandora.com.tr'den sadece kitap seçebilirsiniz");
                }
                
                return Error("İhtiyaç duyduğunuz ürünü seçerken desteklenen platformları kullanın");
            }
            
            return Error("Kampanyanıza ulaşamadık, tekrar deneyin", "There is no campaign to add new item" ,null, 404);
        }
        public class TitleModel
        {
            [Required]
            public Guid Id{get;set;}

            [Required(ErrorMessage = "Bir başlık yazmalısın")]
            [MaxLength(75, ErrorMessage = "Başlık en fazla 75 karakter olmalı")]
            [RegularExpression(@"[a-zA-ZğüşıöçĞÜŞİÖÇ\s,?!]+", ErrorMessage = "A'dan Z'ye harfler, boşluk, virgül, soru işareti ve ünlem girişi yapabilirsiniz")]
            public string Title { get; set; }
        }
        [HttpPatch("Title")]
        public async Task<IActionResult> Title(TitleModel model)
        {
            if(!ModelState.IsValid)
            {
                return Error(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }

            var Need = await Context.Need.Include(need => need.User).FirstOrDefaultAsync(x => 
            x.Id == model.Id 
            && !x.IsRemoved 
            && x.UserId == Guid.Parse(User.Identity.GetUserId()));

            if (Need != null)
            {
                if (Need.Stage == 1)
                {
                    if (model.Title != Need.Title)
                    {
                        var oldTitle = Need.Title;
                        var oldFriendlyTitle = Need.FriendlyTitle;
                        model.Title = model.Title.ClearBlanks();
                        model.Title = model.Title.ToLower().UppercaseFirstCharacters();

                        Need.Title = model.Title;
                        Need.FriendlyTitle = Need.Title.FriendlyUrl();
                        try
                        {
                            await Context.SaveChangesAsync();

                            return Succes("Düzenlendi", new { Need.Link }, 201);
                        }
                        catch (Exception e)
                        {
                            string innerMessage = (e.InnerException != null) ? e.InnerException.Message : "";

                            if (innerMessage.Contains("Unique_Key_Title") || innerMessage.Contains("Unique_Key_FriendlyTitle"))
                            {
                                return Error("Bu başlığı kullanamazsınız");
                            }

                            return Error("Başaramadık, ne olduğunu bilmiyoruz");
                        }
                    }
                    
                    return Error("Hiç bir değişiklik yapılmadı");
                }

                return Error("Bu kampanyada değişiklik yapamazsın", "Stage must be 1");
            }
            
            return Error("Kampanya yok", null, null, 404);
        }

        public class DescriptionModel
        {
            [Required]
            public Guid Id{ get; set; }

            [Required(ErrorMessage = "Bir açıklama yazmalısınız")]
            [MinLength(100, ErrorMessage = "Açıklama en az 100 karakter olmalı")]
            [MaxLength(10000, ErrorMessage = "Açıklama en fazla 10 bin karakter olmalı")]
            [DataType(DataType.MultilineText)]
            public string Description { get; set; }
        }
        [HttpPatch("Description")]
        public async Task<IActionResult> Description(DescriptionModel model)
        {
            if(!ModelState.IsValid)
            {
                return Error(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }

            var Need = await Context.Need.FirstOrDefaultAsync(x => 
            x.Id == model.Id 
            && !x.IsRemoved 
            && x.UserId == Guid.Parse(User.Identity.GetUserId()));

            if (Need != null)
            {
                if (Need.Stage < 4)
                {
                    if (Need.Description != model.Description)
                    {
                        Need.Description = model.Description;
                        await Context.SaveChangesAsync();
                        var description = Need.Description.ReplaceRandNsToBR();
                        
                        return Succes("Düzenlendi", new{ description }, 201);
                    }
                    return Error("Hiç bir değişiklik yapılmadı");
                }
                return Error("Bu kampanyada değişiklik yapamazsın", "Stage must be lower than 4");
            }
            return Error("Kampanya yok", null, null, 404);
        }
    }
}
