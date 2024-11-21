using Ec2Manager.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Ec2Manager
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
            services.AddHealthChecks();
            if (Configuration["OidcAuth:Domain"] != null)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = Configuration["OidcAuth:Domain"];
                    options.RequireHttpsMetadata = true;
                    options.ClientId = Configuration["OidcAuth:ClientId"];
                    options.ClientSecret = Configuration["OidcAuth:ClientSecret"];
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    foreach (string scope in Configuration.GetSection("OidcAuth:ClientScopes").Get<List<string>>())
                    {
                        options.Scope.Add(scope);
                    }
                    options.SaveTokens = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "userrole",
                        ValidateIssuer = true
                    };
                });
                services.AddAuthorization();
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
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });
            app.UseRouting();
            if (Configuration["OidcAuth:Domain"] != null)
            {
                app.UseAuthentication();
            }
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                   name: "Ec2Manager",
                   pattern: "{controller=Ec2Manager}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                   name: "RdsManager",
                   pattern: "{controller=RdsManager}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                   name: "AsgManager",
                   pattern: "{controller=AsgManager}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(name: "Error",
                                                    "error",
                                                    new { controller = "Home", action = "Error" });

                endpoints.MapControllerRoute(name: "PageNotFound",
                                                "pagenotfound",
                                                new { controller = "Home", action = "PageNotFound" });
                endpoints.MapHealthChecks("/healthcheck");
            });
        }
    }
}
