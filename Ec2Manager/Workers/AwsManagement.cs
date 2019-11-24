﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Ec2Manager.Constants;
using Ec2Manager.Interfaces;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;

namespace Ec2Manager.Workers
{
    public static class AwsManagement
    {
        internal static IEnumerable<ClaimValueAccount> LoadClaimValueAccounts(IConfiguration Configuration)
        {
            var claimAccounts = Configuration.GetSection("Okta:ClaimValueAccounts").Get<IEnumerable<ClaimValueAccount>>();
            return claimAccounts;
        }
        internal static IEnumerable<IAwsAccountInfo> LoadAwsAccounts(IConfiguration Configuration)
        {
            var awsKeys = Configuration.GetSection("Ec2Manager:Accounts").Get<IEnumerable<IAwsAccountInfo>>();
            return awsKeys;
        }

        internal static async Task<string> GetSecretKeyFromKmsAsync(IAwsAccountInfo AwsAccountInfo)
        {
            var kmsSecret = string.Empty;
            try
            {
                var accountRegion = RegionEndpoint.GetBySystemName(AwsAccountInfo.Region);
                var kmsClient = new AmazonSecretsManagerClient(accountRegion);
                var kmsRequest = new GetSecretValueRequest()
                {
                    SecretId = AwsAccountInfo.KmsName
                };
                var kmsResponse = await kmsClient.GetSecretValueAsync(kmsRequest);
                kmsSecret = kmsResponse.SecretString;
                kmsClient.Dispose();
                
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(ErrorStrings.ErrorLoadingKmsSecret, AwsAccountInfo.KmsName, ex.Message), ex.InnerException);
            }
            return kmsSecret;
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
                    accountKey.SecretKey = await GetSecretKeyFromKmsAsync(accountKey);
                    var describeRequest = new DescribeInstancesRequest();
                    var ec2Client = new AmazonEC2Client(accountKey.AccessKey, accountKey.SecretKey, accountRegion);
                    var describeResponse = await ec2Client.DescribeInstancesAsync(describeRequest);
                    ec2Client.Dispose();
                    foreach (var reservation in describeResponse.Reservations)
                    {
                        foreach (var instance in reservation.Instances)
                        {
                            if (instance.Tags.Where(t => t.Key == accountKey.TagToSearch).FirstOrDefault() != null)
                            {
                                if (Regex.Match(instance.Tags.SingleOrDefault(t => t.Key == accountKey.TagToSearch)?.Value, accountKey.SearchString).Success)
                                {
                                    var name = instance.Tags.SingleOrDefault(t => t.Key == accountKey.NameTag)?.Value;
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

        internal static async Task StartEc2InstanceAsync(IConfiguration Configuration, string AccountName, string InstanceId)
        {
            try
            {
                var accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                accountKey.SecretKey = await GetSecretKeyFromKmsAsync(accountKey);
                var instanceIdAsList = new List<string> { InstanceId };
                var startRequest = new StartInstancesRequest(instanceIdAsList);
                var ec2Client = new AmazonEC2Client(accountKey.AccessKey, accountKey.SecretKey, accountRegion);
                await ec2Client.StartInstancesAsync(startRequest);
                ec2Client.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }

        internal static async Task RebootEc2InstanceAsync(IConfiguration Configuration, string AccountName, string InstanceId)
        {
            try
            {
                var accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                accountKey.SecretKey = await GetSecretKeyFromKmsAsync(accountKey);
                var instanceIdAsList = new List<string> { InstanceId };
                var rebootRequest = new RebootInstancesRequest(instanceIdAsList);
                var ec2Client = new AmazonEC2Client(accountKey.AccessKey, accountKey.SecretKey, accountRegion);
                await ec2Client.RebootInstancesAsync(rebootRequest);
                ec2Client.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }
    }
}