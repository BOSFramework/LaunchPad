using System;
using System.Net.Http.Headers;
using BOS.Auth.Client.ServiceExtension;
using BOS.LaunchPad.ConfigurationHelpers;
using BOS.LaunchPad.HttpClients;
//using BOS.LaunchPad.HttpClients;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BOS.LaunchPad
{
    public class Startup
    {
        public IConfiguration _Configuration { get; }
        public Startup(IConfiguration _configuration)
        {
            _Configuration = _configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddHttpClient<IIAHttpClient, IAHttpClient>(client =>
            {
                client.BaseAddress = new Uri(_Configuration["BOS:IAUrl"]);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _Configuration["BOS:ApiKey"]);
            });
            services.Configure<ViewConfig>(_Configuration.GetSection("ViewConfiguration"));
            services.AddTransient<IEmailSender>(e => new EmailSender(_Configuration["SendGrid:From"], _Configuration["SendGrid:ApiKey"]));
            services.AddBOSAuthClient(_Configuration["BOS:ApiKey"]);
            services.AddDefaultIdentity<IdentityUser>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(c =>
            {
                c.LoginPath = $"/Identity/Account/Login";
                c.LogoutPath = $"/Identity/Account/Logout";
            });
            services.AddMvc().AddFeatureFolders().AddAreaFeatureFolders();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                
            });
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
