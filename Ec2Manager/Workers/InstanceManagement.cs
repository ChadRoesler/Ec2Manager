using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Amazon.EC2;
using Amazon.EC2.Model;
using Ec2Manager.Constants;
using Ec2Manager.Models;

namespace Ec2Manager.Workers
{
    internal static class InstanceManagement
    {

        private static AwsKey DecryptKeys(AwsKey AwsKey)
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(currentDir, string.Format(ResourceStrings.KeyFileName, AwsKey.AccountName));
            var keyFile = Assembly.LoadFile(path);
            var cryptographyManagement = keyFile.GetType(ResourceStrings.KeyType);
            var cryptographyManagementObject = keyFile.CreateInstance(ResourceStrings.KeyType);
            var decryption = cryptographyManagement.GetMethod(string.Format(ResourceStrings.DecryptionMethodName, AwsKey.AccountName), new Type[] { typeof(string) });
            var encryptedAccessKeyAsObject = new object[] { AwsKey.AccessKey };
            var encryptedSecretKeyAsObject = new object[] { AwsKey.SecretKey };
            var decryptedAccessKey = (string)decryption.Invoke(cryptographyManagementObject, encryptedAccessKeyAsObject);
            var decryptedSecretKey = (string)decryption.Invoke(cryptographyManagementObject, encryptedSecretKeyAsObject);
            AwsKey.AccessKey = decryptedAccessKey;
            AwsKey.SecretKey = decryptedSecretKey;
            return AwsKey;
        }

        internal static List<AwsKey> LoadAwsAccounts(IOptions<AppConfig> Configuration)
        {
            var awsKeys = new List<AwsKey>();
            foreach(var account in  Configuration.Value.Ec2Manager.Accounts)
            {
                awsKeys.Add(new AwsKey(account));
            }
            return awsKeys;
        }

        internal static async Task<List<AwsEc2Instance>> ListEc2Instances(IOptions<AppConfig> Configuration)
        {
            var ecInstancesToManage = new List<AwsEc2Instance>();
            var accounts = LoadAwsAccounts(Configuration);
            foreach (var accountKey in accounts)
            {
                var decryptedAccountKey = DecryptKeys(accountKey);
                var describeRequest = new DescribeInstancesRequest();
                var ec2Client = new AmazonEC2Client(decryptedAccountKey.AccessKey, decryptedAccountKey.SecretKey, decryptedAccountKey.Region);
                var describeResponse = await ec2Client.DescribeInstancesAsync(describeRequest);
                foreach (var reservation in describeResponse.Reservations)
                {
                    foreach (var instance in reservation.Instances)
                    {
                        if (instance.Tags.Where(t => t.Key == decryptedAccountKey.Tag).FirstOrDefault() != null)
                        {
                            if (Regex.Match(instance.Tags.FirstOrDefault(t => t.Key == decryptedAccountKey.Tag).Value, decryptedAccountKey.TagSearchString).Success)
                            {
                                var name = instance.Tags.FirstOrDefault(t => t.Key == decryptedAccountKey.NameTag).Value;
                                var ec2InstanceToManage = new AwsEc2Instance(name, instance.PrivateIpAddress, instance.InstanceId, instance.State.Name.Value, accountKey.AccountName);
                                ecInstancesToManage.Add(ec2InstanceToManage);
                            }
                        }
                    }
                }
            }
            return ecInstancesToManage.OrderBy(x => x.Name).ToList();
        }

        internal static async Task StartEc2Instance(IOptions<AppConfig> Configuration, string AccountName, string InstanceId)
        {
            var accountKey = LoadAwsAccounts(Configuration).Where(x => x.AccountName == AccountName).FirstOrDefault();
            var decryptedAccountKey = DecryptKeys(accountKey);
            var instanceIdAsList = new List<string> { InstanceId };
            var startRequest = new StartInstancesRequest(instanceIdAsList);
            var ec2Client = new AmazonEC2Client(decryptedAccountKey.AccessKey, decryptedAccountKey.SecretKey, decryptedAccountKey.Region);
            await ec2Client.StartInstancesAsync(startRequest);
        }

        internal static async Task RebootEc2Instance(IOptions<AppConfig> Configuration, string AccountName, string InstanceId)
        {
            var accountKey = LoadAwsAccounts(Configuration).Where(x => x.AccountName == AccountName).FirstOrDefault();
            var decryptedAccountKey = DecryptKeys(accountKey);
            var instanceIdAsList = new List<string> { InstanceId };
            var rebootRequest = new RebootInstancesRequest(instanceIdAsList);
            var ec2Client = new AmazonEC2Client(decryptedAccountKey.AccessKey, decryptedAccountKey.SecretKey, decryptedAccountKey.Region);
            await ec2Client.RebootInstancesAsync(rebootRequest);
        }
    }
}
