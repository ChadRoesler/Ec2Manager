using System.Collections.Generic;
using Ec2Manager.Models.ConfigManagement;

namespace Ec2Manager.Interfaces
{
    interface IOktaConfig
    {
        public string OktaDomain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public IEnumerable<string> ClientScopes { get; set; }

        public string ClientAccountManagementClaim { get; set; }
        public IEnumerable<ClaimValueAccount> ClaimValueAccounts  { get; set; }
}
}
