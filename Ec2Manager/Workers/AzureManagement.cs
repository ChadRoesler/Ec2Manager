using Ec2Manager.Constants;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ec2Manager.Workers
{
    public static class AzureManagement
    {
        internal static IEnumerable<AzureAccountInfo> LoadAwsAccounts(IConfiguration Configuration)
        {
            var azureKeys = Configuration.GetSection("Ec2Manager:Accounts:Azure").Get<IEnumerable<AzureAccountInfo>>();
            return azureKeys;
        }


    }
}
