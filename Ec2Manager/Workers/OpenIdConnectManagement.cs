using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Ec2Manager.Models.ConfigManagement;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Ec2Manager.Constants;
using System.Text.RegularExpressions;

namespace Ec2Manager.Workers
{
    public static class OpenIdConnectManagement
    {
        internal static IEnumerable<ClaimValueAccount> LoadClaimValueAccounts(IConfiguration Configuration)
        {
            var claimAccounts = Configuration.GetSection("OidcAuth:ClaimValueAccounts").Get<IEnumerable<ClaimValueAccount>>();
            return claimAccounts;
        }

        internal static IEnumerable<ClaimValueAccount> LoadClaimValueAccounts(IConfiguration Configuration, IHttpContextAccessor HttpContextAccessor)
        {
            IEnumerable<ClaimValueAccount> claimAccounts;
            var userClaims = HttpContextAccessor.HttpContext.User.Claims;
            if (Configuration.GetValue<string>("OidcAuth:ClientAccountManagementClaim") != null)
            {
                var userClaimAccountManagement = userClaims.Where(x => x.Type == Configuration.GetValue<string>("OidcAuth:ClientAccountManagementClaim") && !Regex.Match(x.Value, "\\[.*\\]", RegexOptions.IgnoreCase).Success);
                var userAccounts = new List<string>();
                userAccounts.AddRange(userClaimAccountManagement.Select(x => x.Value));
                claimAccounts = Configuration.GetSection("OidcAuth:ClaimValueAccounts").Get<IEnumerable<ClaimValueAccount>>().Where(x => userAccounts.Contains(x.Value));
            }
            else
            {
                claimAccounts = ( new [] {new ClaimValueAccount
                {
                    Value = "NoClaims",
                    Accounts = AwsManagement.LoadAwsAccounts(Configuration).Select(x => x.AccountName),
                    EnableStart = Configuration.GetValue<bool>("Ec2Manager:EnableStart"),
                    EnableReboot = Configuration.GetValue<bool>("Ec2Manager:EnableReboot"),
                    EnableStop = Configuration.GetValue<bool>("Ec2Manager:EnableStop"),
                    AdminAccess = Configuration.GetValue<bool>("Ec2Manager:AdminAccess")
                } });
            }
            return claimAccounts;
        }
    }
}
