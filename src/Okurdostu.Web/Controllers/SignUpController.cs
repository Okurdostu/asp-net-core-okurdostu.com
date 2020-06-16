using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Okurdostu.Data.Model;
using Okurdostu.Data.Model.Context;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Models;

namespace Okurdostu.Web.Controllers
{
    public class SignUpController : Controller
    {
        [Route("~/Kaydol")]
        public IActionResult Index()
        {
            return View();
        }
        private readonly OkurdostuContext Context;
        public SignUpController(OkurdostuContext _context) => Context = _context;

        [HttpPost, ValidateAntiForgeryToken]
        [Route("~/Kaydol")]
        public async Task<IActionResult> Index(SignUpModel Model)
        {
            if (ModelState.IsValid)
            {
                var User = new User
                {
                    Username = Model.Username,
                    Email = Model.Email,
                    Password = Model.Password.SHA512(),
                    FullName = Model.FullName,
                };
                try
                {
                    await Context.User.AddAsync(User);
                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {
                        //coming events: login with created user and after that return the beta page
                    }
                    else
                    {
                        //coming events: feedback(SignUpMessage)
                    }
                }
                catch (Exception e)
                {
                    if (e.InnerException.Message.Contains("user_username_uindex"))
                        TempData["SignUpMessage"] = "Bu kullanıcı adını kullanamazsınız";
                    else if (e.InnerException.Message.Contains("user_email_uindex"))
                        TempData["SignUpMessage"] = "Bu e-mail adresini kullanamazsınız";
                    else
                        TempData["SignUpMessage"] = "Başaramadık ne olduğunu bilmiyoruz";
                }
            }
            //feedback(SignUpMessage)
            return View();
        }
    }
}
