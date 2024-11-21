using Ec2Manager.Models.ConfigManagement;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Ec2Manager.Workers
{
    /// <summary>
    /// Provides methods for managing OpenID Connect configurations.
    /// </summary>
    public static class OpenIdConnectManagement
    {
        /// <summary>
        /// Loads the claim value accounts from the configuration.
        /// </summary>
        /// <param name="Configuration">The configuration instance to load the claim value accounts from.</param>
        /// <returns>An enumerable of <see cref="ClaimValueAccount"/>.</returns>
        internal static IEnumerable<ClaimValueAccount> LoadClaimValueAccounts(IConfiguration Configuration)
        {
            return Configuration.GetSection("OidcAuth:ClaimValueAccounts").Get<IEnumerable<ClaimValueAccount>>();
        }
    }
}
