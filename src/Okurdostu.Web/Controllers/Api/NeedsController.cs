using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Models;
using Okurdostu.Web.Models.NeedItem;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    public class NeedsController : SecureApiController
    {
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
                IsRemoved = false,
                IsWrong = false
            };

            await Context.AddAsync(NeedItem);
            NeedItem.Need.TotalCharge += NeedItem.Price;
            await Context.SaveChangesAsync();
        }

        //post: /api/needs/item
        [HttpPost("item")]
        public async Task<IActionResult> AddItem(string itemLink, Guid needId)
        {
            ReturnModel rm = new ReturnModel();
            sbyte maxItemCount = 3;
            var AuthenticatedUserId = Guid.Parse(User.Identity.GetUserId());

            var Need = await Context.Need.Where(x => x.Id == needId
            && x.UserId == AuthenticatedUserId
            && !x.IsRemoved
            && !x.IsSentForConfirmation
            && x.NeedItem.Where(a => a.IsRemoved != true).Count() < maxItemCount)
                .FirstOrDefaultAsync();

            if (Need != null)
            {
                if (itemLink.Contains("udemy.com"))
                {
                    Udemy Udemy = new Udemy();
                    Udemy = Udemy.Product(itemLink);
                    if (Udemy.Error == null)
                    {
                        await AddNeedItem(Need.Id, Udemy.Link, Udemy.Name, Udemy.Price, "/image/udemy.png", "Udemy").ConfigureAwait(false);

                        //rm.Data = Udemy;
                        return Succes(rm);
                    }
                    else
                    {
                        Logger.LogError("Udemy Error:{error}, Link:{link}", Udemy.Error, itemLink);
                        rm.Message = Udemy.Error;
                        return Error(rm);
                    }
                }
                else if (itemLink.Contains("amazon.com.tr"))
                {
                    Amazon Amazon = new Amazon();
                    Amazon = Amazon.Product(itemLink);
                    if (Amazon.Error == null)
                    {
                        await AddNeedItem(Need.Id, Amazon.Link, Amazon.Name, Amazon.Price, "/image/amazon.png", "Amazon").ConfigureAwait(false);

                        //rm.Data = Amazon;
                        return Succes(rm);
                    }
                    else
                    {
                        Logger.LogError("Amazon Error:{error}, Link:{link}", Amazon.Error, itemLink);
                        rm.Message = Amazon.Error;
                        return Error(rm);
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

                            //rm.Data = Pandora;
                            return Succes(rm);
                        }
                        else
                        {
                            Logger.LogError("Pandora Error:{error}, Link:{link}", Pandora.Error, itemLink);
                            rm.Message = Pandora.Error;
                            return Error(rm);
                        }
                    }
                    else
                    {
                        rm.Message = "Pandora.com.tr'den sadece kitap seçebilirsiniz";
                        return Error(rm);
                    }
                }
                else
                {
                    rm.Message = "İhtiyaç duyduğunuz ürünü seçerken desteklenen platformları kullanın";
                    return Error(rm);
                }
            }
            else
            {
                rm.Message = "Kampanyanıza ulaşamadık, tekrar deneyin";
                rm.InternalMessage = "There isn't campaign to add a new item";

                return Error(rm);
            }
        }
    }
}
