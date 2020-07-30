using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Okurdostu.Data;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Services;
using System.Linq;

namespace Okurdostu.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.IgnoreNullValues = true;
                 options.JsonSerializerOptions.MaxDepth = 64;
             });

            services.AddRazorPages();
            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("Email").Get<EmailConfiguration>());

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "okurdostu-authentication";
                    options.LoginPath = "/girisyap";
                    options.AccessDeniedPath = "/";

                    options.ExpireTimeSpan = System.TimeSpan.FromDays(1);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                });
            services.AddMemoryCache();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ConfirmedEmailFilter>();
            services.AddControllersWithViews();
            services.AddDbContext<OkurdostuContext>(option => option.UseSqlServer(Configuration.GetConnectionString("OkurdostuLocal")));


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");

                endpoints.MapControllerRoute(
                    name: "usernameredirect",
                    pattern: "{username}",
                    constraints: new { user = new UserConstraint() },
                    defaults: new { controller = "Profile", action = "Index", });
            });
        }
        public class UserConstraint : IRouteConstraint
        {
            public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
            {
                var ValueFromRoute = values["username"].ToString().ToLower();

                var Context = (OkurdostuContext)httpContext?.RequestServices.GetService(typeof(OkurdostuContext));

                string[] blockedRouteValues = {
                    "api","","comment","account","confirmemail","girisyap","kaydol","ihtiyaclar","beta","arama",
                    "gizlilik-politikasi","kullanici-sozlesmesi","sss","kvkk",
                    "home", "like","logout", "ihtiyac-olustur","ihtiyac", "universiteler"
                };

                bool IsComingValueEqualAnyBlockedRoute = blockedRouteValues.Any(x => x == ValueFromRoute || x.Contains(ValueFromRoute));

                if (IsComingValueEqualAnyBlockedRoute)
                {
                    return !IsComingValueEqualAnyBlockedRoute;
                }
                else
                {
                    var Usernames = Context.User.Select(x => new
                    {
                        x.Username
                    }).ToList();
                    return Usernames.Any(x => x.Username.ToLower() == ValueFromRoute);
                }
            }
        }
    }
}
