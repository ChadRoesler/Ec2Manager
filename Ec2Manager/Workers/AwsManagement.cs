using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
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
    public static class AwsManagement
    {

        internal static IEnumerable<AwsAccountInfo> LoadAwsAccounts(IConfiguration Configuration)
        {
            var awsKeys = Configuration.GetSection("Ec2Manager:Accounts").Get<IEnumerable<AwsAccountInfo>>();
            return awsKeys;
        }

        //internal static async Task<string> GetSecretKeyAsync(IAwsAccountInfo AwsAccountInfo)
        //{
        //    var secretsManagerSecret = string.Empty;
        //    try
        //    {
        //        var accountRegion = RegionEndpoint.GetBySystemName(AwsAccountInfo.Region);
        //        var secretsManagerClient = new AmazonSecretsManagerClient(accountRegion);
        //        var secretsManagerRequest = new GetSecretValueRequest()
        //        {
        //            SecretId = AwsAccountInfo.SecretName
        //        };
        //        var secretsManagerResponse = await secretsManagerClient.GetSecretValueAsync(secretsManagerRequest);
        //        secretsManagerSecret = secretsManagerResponse.SecretString;
        //        secretsManagerClient.Dispose();

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(string.Format(ErrorStrings.ErrorLoadingSecret, AwsAccountInfo.SecretName, ex.Message), ex.InnerException);
        //    }
        //    return secretsManagerSecret;
        //}

        internal static async Task<List<Ec2Instance>> ListEc2InstancesAsync(IConfiguration Configuration, string User)
        {
            var ecInstancesToManage = new List<Ec2Instance>();
            var accounts = LoadAwsAccounts(Configuration);

            foreach (var accountKey in accounts)
            {
                try
                {
                    var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                    //accountKey.SecretKey = await GetSecretKeyAsync(accountKey);
                    var stsClient = new AmazonSecurityTokenServiceClient();
                    var assumeRoleRequest = new AssumeRoleRequest
                    {
                        RoleArn = accountKey.RoleArn,
                        RoleSessionName = string.Format(ResourceStrings.ListAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString()),
                        DurationSeconds = 900
                    };
                    var stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                    var describeRequest = new DescribeInstancesRequest();
                    var ec2Client = new AmazonEC2Client(stsResponse.Credentials, accountRegion);
                    var describeResponse = await ec2Client.DescribeInstancesAsync(describeRequest);
                    ec2Client.Dispose();
                    stsClient.Dispose();
                    foreach (var reservation in describeResponse.Reservations)
                    {
                        foreach (var instance in reservation.Instances)
                        {
                            if (instance.Tags.Where(t => t.Key == accountKey.TagToSearch).FirstOrDefault() != null)
                            {
                                if (Regex.Match(instance.Tags.SingleOrDefault(t => t.Key == accountKey.TagToSearch)?.Value, accountKey.SearchString).Success)
                                {
                                    var name = instance.Tags.SingleOrDefault(t => t.Key == accountKey.NameTag)?.Value;
                                    var state = string.Empty;
                                    if (instance.State.Name == InstanceStateName.Running)
                                    {
                                        stsClient = new AmazonSecurityTokenServiceClient();
                                        stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                                        ec2Client = new AmazonEC2Client(stsResponse.Credentials, accountRegion);
                                        //ec2Client = new AmazonEC2Client(accountKey.AccessKey, accountKey.SecretKey, accountRegion);
                                        var statusResponse = await ec2Client.DescribeInstanceStatusAsync(new DescribeInstanceStatusRequest { InstanceIds = { instance.InstanceId } });
                                        ec2Client.Dispose();
                                        stsClient.Dispose();
                                        if (string.Equals(statusResponse.InstanceStatuses[0].Status.Status.Value, "impaired", StringComparison.InvariantCultureIgnoreCase) || string.Equals(statusResponse.InstanceStatuses[0].SystemStatus.Status.Value, "impaired", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            state = "impaired";
                                        }
                                        else if (string.Equals(statusResponse.InstanceStatuses[0].Status.Status.Value, "initializing", StringComparison.InvariantCultureIgnoreCase) || string.Equals(statusResponse.InstanceStatuses[0].SystemStatus.Status.Value, "initializing", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            state = "initializing";
                                        }
                                        else
                                        {
                                            state = instance.State.Name.Value;
                                        }
                                    }
                                    else
                                    {
                                        state = instance.State.Name.Value;
                                    }
                                    var ec2InstanceToManage = new Ec2Instance(name, instance.PrivateIpAddress, instance.InstanceId, state, accountKey.AccountName);
                                    ecInstancesToManage.Add(ec2InstanceToManage);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(ErrorStrings.ErrorLoadingAccount, accountKey.AccountName, ex.Message), ex);
                }
            }
            return ecInstancesToManage.OrderBy(x => x.Name).ToList();
        }

        internal static async Task<StartInstancesResponse> StartEc2InstanceAsync(IConfiguration Configuration, string User, string AccountName, string InstanceId)
        {
            try
            {
                var accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                //accountKey.SecretKey = await GetSecretKeyAsync(accountKey);
                var stsClient = new AmazonSecurityTokenServiceClient();
                var assumeRoleRequest = new AssumeRoleRequest
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = string.Format(ResourceStrings.StartAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString()),
                    DurationSeconds = 900
                };
                var stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                var instanceIdAsList = new List<string> { InstanceId };
                var startRequest = new StartInstancesRequest(instanceIdAsList);
                //var ec2Client = new AmazonEC2Client(accountKey.AccessKey, accountKey.SecretKey, accountRegion);
                var ec2Client = new AmazonEC2Client(stsResponse.Credentials, accountRegion);
                var response = ec2Client.StartInstancesAsync(startRequest);
                ec2Client.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }

        internal static async Task<RebootInstancesResponse> RebootEc2InstanceAsync(IConfiguration Configuration, string User, string AccountName, string InstanceId)
        {
            try
            {
                var accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                //accountKey.SecretKey = await GetSecretKeyAsync(accountKey);
                var stsClient = new AmazonSecurityTokenServiceClient();
                var assumeRoleRequest = new AssumeRoleRequest
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = string.Format(ResourceStrings.RebootAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString()),
                    DurationSeconds = 900
                };
                var stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                var instanceIdAsList = new List<string> { InstanceId };
                var rebootRequest = new RebootInstancesRequest(instanceIdAsList);
                //var ec2Client = new AmazonEC2Client(accountKey.AccessKey, accountKey.SecretKey, accountRegion);
                var ec2Client = new AmazonEC2Client(stsResponse.Credentials, accountRegion);
                var response = ec2Client.RebootInstancesAsync(rebootRequest);
                ec2Client.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }

        internal static async Task<StopInstancesResponse> StopEc2InstanceAsync(IConfiguration Configuration, string User, string AccountName, string InstanceId)
        {
            try
            {
                var accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                var accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                //accountKey.SecretKey = await GetSecretKeyAsync(accountKey);
                var stsClient = new AmazonSecurityTokenServiceClient();
                var assumeRoleRequest = new AssumeRoleRequest
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = string.Format(ResourceStrings.StopAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString()),
                    DurationSeconds = 900
                };
                var stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                var instanceIdAsList = new List<string> { InstanceId };
                var stopRequest = new StopInstancesRequest(instanceIdAsList);
                //var ec2Client = new AmazonEC2Client(accountKey.AccessKey, accountKey.SecretKey, accountRegion);
                var ec2Client = new AmazonEC2Client(stsResponse.Credentials, accountRegion);
                var response = ec2Client.StopInstancesAsync(stopRequest);
                ec2Client.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }
    }
}
