using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Amazon.EC2;
using Amazon.EC2.Model;
using Ec2Manager.Constants;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;
using Microsoft.Extensions.Configuration;
using Amazon;

namespace Ec2Manager.Workers
{
    public static class InstanceManagement
    {
        internal static IEnumerable<ClaimValueAccount> LoadClaimValueAccounts(IConfiguration Configuration)
        {
            var claimAccounts = Configuration.GetSection("Okta:ClaimValueAccounts").Get<IEnumerable<ClaimValueAccount>>();
            return claimAccounts;
        }
        internal static IEnumerable<AwsAccountInfo> LoadAwsAccounts(IConfiguration Configuration)
        {
            var awsKeys = Configuration.GetSection("Ec2Manager:Accounts").Get<IEnumerable<AwsAccountInfo>>();
            return awsKeys;
        }

        internal static async Task<List<Ec2Instance>> ListEc2InstancesAsync(IConfiguration Configuration)
        {
            var ecInstancesToManage = new List<Ec2Instance>();
            var accounts = LoadAwsAccounts(Configuration);
            foreach (var accountKey in accounts)
            {
                try
                {
                    var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                    var decryptedAccountKey = KeyCryptography.DecryptKeys(accountKey);
                    var describeRequest = new DescribeInstancesRequest();
                    var ec2Client = new AmazonEC2Client(decryptedAccountKey.AccessKey, decryptedAccountKey.SecretKey, accountRegion);
                    var describeResponse = await ec2Client.DescribeInstancesAsync(describeRequest);
                    ec2Client.Dispose();
                    foreach (var reservation in describeResponse.Reservations)
                    {
                        foreach (var instance in reservation.Instances)
                        {
                            if (instance.Tags.Where(t => t.Key == decryptedAccountKey.TagToSearch).FirstOrDefault() != null)
                            {
                                if (Regex.Match(instance.Tags.SingleOrDefault(t => t.Key == decryptedAccountKey.TagToSearch)?.Value, decryptedAccountKey.SearchString).Success)
                                {
                                    var name = instance.Tags.SingleOrDefault(t => t.Key == decryptedAccountKey.NameTag)?.Value;
                                    var ec2InstanceToManage = new Ec2Instance(name, instance.PrivateIpAddress, instance.InstanceId, instance.State.Name.Value, accountKey.AccountName);
                                    ecInstancesToManage.Add(ec2InstanceToManage);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception(string.Format(ErrorStrings.ErrorLoadingAccount, accountKey.AccountName, ex.Message), ex);
                }
            }
            return ecInstancesToManage.OrderBy(x => x.Name).ToList();
        }

        internal static void StartEc2Instance(IConfiguration Configuration, string AccountName, string InstanceId)
        {
            try
            {
                var accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                var decryptedAccountKey = KeyCryptography.DecryptKeys(accountKey);
                var instanceIdAsList = new List<string> { InstanceId };
                var startRequest = new StartInstancesRequest(instanceIdAsList);
                var ec2Client = new AmazonEC2Client(decryptedAccountKey.AccessKey, decryptedAccountKey.SecretKey, accountRegion);
                ec2Client.StartInstancesAsync(startRequest);
                ec2Client.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }

        internal static void RebootEc2Instance(IConfiguration Configuration, string AccountName, string InstanceId)
        {
            try
            {
                var accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                var decryptedAccountKey = KeyCryptography.DecryptKeys(accountKey);
                var instanceIdAsList = new List<string> { InstanceId };
                var rebootRequest = new RebootInstancesRequest(instanceIdAsList);
                var ec2Client = new AmazonEC2Client(decryptedAccountKey.AccessKey, decryptedAccountKey.SecretKey, accountRegion);
                ec2Client.RebootInstancesAsync(rebootRequest);
                ec2Client.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }
    }
}
