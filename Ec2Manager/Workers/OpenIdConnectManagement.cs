using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Ec2Manager.Models.ConfigManagement;

namespace Ec2Manager.Workers
{
    public static class OpenIdConnectManagement
    {
        internal static IEnumerable<ClaimValueAccount> LoadClaimValueAccounts(IConfiguration Configuration)
        {
            var claimAccounts = Configuration.GetSection("OidcAuth:ClaimValueAccounts").Get<IEnumerable<ClaimValueAccount>>();
            return claimAccounts;
        }
    }
}
