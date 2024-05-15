using Ec2Manager.Models.ConfigManagement;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Ec2Manager.Workers
{
    public static class OpenIdConnectManagement
    {
        internal static IEnumerable<ClaimValueAccount> LoadClaimValueAccounts(IConfiguration Configuration)
        {
            IEnumerable<ClaimValueAccount> claimAccounts = Configuration.GetSection("OidcAuth:ClaimValueAccounts").Get<IEnumerable<ClaimValueAccount>>();
            return claimAccounts;
        }
    }
}
