using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Okta.AspNetCore;
using HealthChecks.UI.Client;
using Ec2Manager.Models;
using Microsoft.Extensions.Logging;


namespace Ec2Manager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddHealthChecks();
            if (Configuration.GetValue<string>("Okta:OktaDomain") != null)
            {
                services.AddAuthorizationCore();
                services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OktaDefaults.MvcAuthenticationScheme;
                    //sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                //.AddOpenIdConnect(options =>
                //{
                //    options.ClientId = Configuration["Okta:ClientId"];
                //    options.ClientSecret = Configuration["Okta:ClientSecret"];
                //    options.Authority = Configuration["Okta:OktaDomain"];
                //    options.CallbackPath = "/authorization-code/callback";
                //    options.ResponseType = "code";
                //    options.SaveTokens = true;
                //    options.UseTokenLifetime = false;
                //    options.GetClaimsFromUserInfoEndpoint = true;
                //    options.Scope.Add("openid");
                //    options.Scope.Add("profile");
                //    options.TokenValidationParameters = new TokenValidationParameters
                //    {
                //        NameClaimType = "name"
                //    };
                //});
                .AddOktaMvc(new OktaMvcOptions
                {
                    OktaDomain = Configuration.GetValue<string>("Okta:OktaDomain"),
                    ClientId = Configuration.GetValue<string>("Okta:ClientId"),
                    ClientSecret = Configuration.GetValue<string>("Okta:ClientSecret"),
                    Scope = Configuration.GetValue<List<string>>("Okta:ClientScopes")
                });           
            }
            else
            {
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAssertion(_ => true)
                        .Build();
                });
            }
            services.AddControllersWithViews();
            services.AddMvc(options => { options.EnableEndpointRouting = false; }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory log)
        {
            log.AddLog4Net();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseStatusCodePages();
                app.UseMiddleware<ErrorHandler>();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseHealthChecks("/healthcheck", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.UseRouting();
            app.UseAuthorization();
            if (Configuration.GetValue<string>("Okta:OktaDomain") != null)
            {
                app.UseAuthentication();
            }

            ///////////////////////////////////////////
            /// Commented out until Okta asp.net 3.0
            /////////////////////////////////////////

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //    endpoints.MapControllerRoute(name: "Error",
            //                                        "error",
            //                                        new { controller = "Home", action = "Error" });

            //    endpoints.MapControllerRoute(name: "PageNotFound",
            //                                    "pagenotfound",
            //                                    new { controller = "Home", action = "PageNotFound" });
            //});
            app.UseMvc(routes =>
            {

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(name: "Error",
                                    "error",
                                    new { controller = "Home", action = "Error" });
                routes.MapRoute(name: "PageNotFound",
                                                "pagenotfound",
                                                new { controller = "Home", action = "PageNotFound" });
            });
        }
    }
}
