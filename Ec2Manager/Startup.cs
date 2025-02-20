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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ec2Manager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            var oidcDomain = Configuration["OidcAuth:Domain"] ?? string.Empty;
            if (!string.IsNullOrEmpty(oidcDomain))
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Events.OnSigningIn = async context =>
                    {
                        var claimsIdentity = (ClaimsIdentity)context.Principal.Identity;
                        var claimsToKeep = new List<string> { "name", "userrole", "preferred_username" }; // Add essential claims here
                        var claimsToRemove = claimsIdentity.Claims
                            .Where(claim => !claimsToKeep.Contains(claim.Type))
                            .ToList();

                        foreach (var claim in claimsToRemove)
                        {
                            claimsIdentity.RemoveClaim(claim);
                        }

                        await Task.CompletedTask;
                    };
                })
                .AddOpenIdConnect(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = oidcDomain;
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
                services.AddAuthorizationBuilder()
                    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                        .RequireAssertion(_ => true)
                        .Build());
            }
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory log)
        {
            log.AddLog4Net();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
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
            if (!string.IsNullOrEmpty(Configuration["OidcAuth:Domain"]))
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
                                             pattern: "error",
                                             defaults: new { controller = "Home", action = "Error" });
                endpoints.MapControllerRoute(name: "PageNotFound",
                                             pattern: "pagenotfound",
                                             defaults: new { controller = "Home", action = "PageNotFound" });
                endpoints.MapHealthChecks("/healthcheck");
            });
        }
    }
}
