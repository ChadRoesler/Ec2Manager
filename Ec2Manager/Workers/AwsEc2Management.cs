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
    public static class AwsEc2Management
    {

        internal static IEnumerable<Ec2AwsAccountInfo> LoadEc2AwsAccounts(IConfiguration Configuration)
        {
            IEnumerable<Ec2AwsAccountInfo> awsKeys = Configuration.GetSection("Ec2Manager:AwsAccounts").Get<IEnumerable<Ec2AwsAccountInfo>>();
            return awsKeys;
        }

        internal static async Task<List<Ec2Instance>> ListEc2InstancesAsync(IConfiguration Configuration, string User)
        {
            List<Ec2Instance> ec2InstancesToManage = new();
            IEnumerable<Ec2AwsAccountInfo> accounts = LoadEc2AwsAccounts(Configuration);

            foreach (Ec2AwsAccountInfo accountKey in accounts)
            {
                try
                {
                    RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                    AmazonSecurityTokenServiceClient stsClient = new();
                    string sessionName = string.Format(ResourceStrings.ListAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                    sessionName = sessionName.Length > 63 ? sessionName.Substring(0, 63) : sessionName;
                    AssumeRoleRequest assumeRoleRequest = new()
                    {
                        RoleArn = accountKey.RoleArn,
                        RoleSessionName = sessionName,
                        DurationSeconds = 900
                    };
                    AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                    DescribeInstancesRequest describeRequest = new();
                    AmazonEC2Client ec2Client = new(stsResponse.Credentials, accountRegion);
                    DescribeInstancesResponse describeResponse = await ec2Client.DescribeInstancesAsync(describeRequest);
                    ec2Client.Dispose();
                    stsClient.Dispose();
                    foreach (Reservation reservation in describeResponse.Reservations)
                    {
                        foreach (Instance instance in reservation.Instances)
                        {
                            if (instance.Tags.Where(t => t.Key == accountKey.TagToSearch).FirstOrDefault() != null)
                            {
                                if (Regex.Match(instance.Tags.SingleOrDefault(t => t.Key == accountKey.TagToSearch)?.Value, accountKey.SearchString).Success)
                                {
                                    string name = instance.Tags.SingleOrDefault(t => t.Key == accountKey.NameTag)?.Value;
                                    string state = string.Empty;
                                    if (instance.State.Name == InstanceStateName.Running)
                                    {
                                        stsClient = new AmazonSecurityTokenServiceClient();
                                        stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                                        ec2Client = new AmazonEC2Client(stsResponse.Credentials, accountRegion);
                                        DescribeInstanceStatusResponse statusResponse = await ec2Client.DescribeInstanceStatusAsync(new DescribeInstanceStatusRequest { InstanceIds = { instance.InstanceId } });
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
                                    Ec2Instance ec2InstanceToManage = new(name, instance.PrivateIpAddress, instance.InstanceId, state, accountKey.AccountName);
                                    ec2InstancesToManage.Add(ec2InstanceToManage);
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
            return ec2InstancesToManage.OrderBy(x => x.Name).ToList();
        }

        internal static async Task<StartInstancesResponse> StartEc2InstanceAsync(IConfiguration Configuration, string User, string AccountName, string InstanceId)
        {
            try
            {
                Ec2AwsAccountInfo accountKey = LoadEc2AwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                AmazonSecurityTokenServiceClient stsClient = new();
                string sessionName = string.Format(ResourceStrings.StartAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                sessionName = sessionName.Length > 63 ? sessionName.Substring(0, 63) : sessionName;
                AssumeRoleRequest assumeRoleRequest = new()
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = sessionName,
                    DurationSeconds = 900
                };
                AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                List<string> instanceIdAsList = new() { InstanceId };
                StartInstancesRequest startRequest = new(instanceIdAsList);
                AmazonEC2Client ec2Client = new(stsResponse.Credentials, accountRegion);
                Task<StartInstancesResponse> response = ec2Client.StartInstancesAsync(startRequest);
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
                Ec2AwsAccountInfo accountKey = LoadEc2AwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                AmazonSecurityTokenServiceClient stsClient = new();
                string sessionName = string.Format(ResourceStrings.RebootAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                sessionName = sessionName.Length > 63 ? sessionName.Substring(0, 63) : sessionName;
                AssumeRoleRequest assumeRoleRequest = new()
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = sessionName,
                    DurationSeconds = 900
                };
                AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                List<string> instanceIdAsList = new() { InstanceId };
                RebootInstancesRequest rebootRequest = new(instanceIdAsList);
                AmazonEC2Client ec2Client = new(stsResponse.Credentials, accountRegion);
                Task<RebootInstancesResponse> response = ec2Client.RebootInstancesAsync(rebootRequest);
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
                Ec2AwsAccountInfo accountKey = LoadEc2AwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                AmazonSecurityTokenServiceClient stsClient = new();
                string sessionName = string.Format(ResourceStrings.StopAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                sessionName = sessionName.Length > 63 ? sessionName.Substring(0, 63) : sessionName;
                AssumeRoleRequest assumeRoleRequest = new()
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = sessionName,
                    DurationSeconds = 900
                };
                AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                List<string> instanceIdAsList = new() { InstanceId };
                StopInstancesRequest stopRequest = new(instanceIdAsList);
                AmazonEC2Client ec2Client = new(stsResponse.Credentials, accountRegion);
                Task<StopInstancesResponse> response = ec2Client.StopInstancesAsync(stopRequest);
                ec2Client.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StopEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }

    }
}
