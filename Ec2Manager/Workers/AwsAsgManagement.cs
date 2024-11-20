using Amazon;
using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
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
    public static class AwsAsgManagement
    {
        internal static IEnumerable<AsgAwsAccountInfo> LoadAsgAwsAccounts(IConfiguration Configuration)
        {
            IEnumerable<AsgAwsAccountInfo> awsKeys = Configuration.GetSection("AsgManager:AwsAccounts").Get<IEnumerable<AsgAwsAccountInfo>>();
            return awsKeys;
        }

        internal static async Task<List<AsGroup>> ListAsGroupsAsync(IConfiguration Configuration, string User)
        {
            List<AsGroup> asGroupsToManage = new();
            IEnumerable<AsgAwsAccountInfo> accounts = LoadAsgAwsAccounts(Configuration);

            foreach (AsgAwsAccountInfo accountKey in accounts)
            {
                try
                {
                    RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                    AmazonSecurityTokenServiceClient stsClient = new();
                    string sessionName = string.Format(ResourceStrings.ListAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                    sessionName = sessionName.Length > 63 ? sessionName[..63] : sessionName;
                    AssumeRoleRequest assumeRoleRequest = new()
                    {
                        RoleArn = accountKey.RoleArn,
                        RoleSessionName = sessionName,
                        DurationSeconds = 900
                    };
                    AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                    DescribeAutoScalingGroupsRequest describeRequest = new();
                    AmazonAutoScalingClient asgClient = new(stsResponse.Credentials, accountRegion);
                    DescribeAutoScalingGroupsResponse describeResponse = await asgClient.DescribeAutoScalingGroupsAsync(describeRequest);
                    asgClient.Dispose();
                    stsClient.Dispose();
                    foreach (AutoScalingGroup group in describeResponse.AutoScalingGroups)
                    {
                        if (group.Tags.Where(t => t.Key == accountKey.TagToSearch).FirstOrDefault() != null)
                        {
                            if (Regex.Match(group.Tags.SingleOrDefault(t => t.Key == accountKey.TagToSearch)?.Value, accountKey.SearchString).Success)
                            {
                                AssumeRoleRequest assumeRoleRequestIr = new()
                                {
                                    RoleArn = accountKey.RoleArn,
                                    RoleSessionName = sessionName,
                                    DurationSeconds = 900
                                };
                                stsClient = new AmazonSecurityTokenServiceClient();
                                int.TryParse(group.Tags.SingleOrDefault(t => t.Key == accountKey.DesiredCapacityTag)?.Value ?? "1", out int desiredCapacity);
                                int.TryParse(group.Tags.SingleOrDefault(t => t.Key == accountKey.MaxCapacityTag)?.Value ?? "1", out int maxCapacity);
                                int.TryParse(group.Tags.SingleOrDefault(t => t.Key == accountKey.MinCapacityTag)?.Value ?? "1", out int minCapacity);
                                AssumeRoleResponse stsResponseIr = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                                DescribeInstanceRefreshesRequest describeIrRequest = new() { AutoScalingGroupName = group.AutoScalingGroupName };
                                asgClient = new(stsResponseIr.Credentials, accountRegion);
                                DescribeInstanceRefreshesResponse describeResponseIr = await asgClient.DescribeInstanceRefreshesAsync(describeIrRequest);
                                asgClient.Dispose();
                                stsClient.Dispose();
                                bool refreshStatus = (describeResponseIr.InstanceRefreshes.OrderBy(x => x.StartTime).ToArray().FirstOrDefault() != null ? (describeResponseIr.InstanceRefreshes.OrderBy(x => x.StartTime).ToArray().FirstOrDefault()?.Status == InstanceRefreshStatus.InProgress) : false);
                                AsGroup asGroupToManage = new(group.AutoScalingGroupName, group.Instances.Count, refreshStatus, accountKey.AccountName, desiredCapacity, maxCapacity, minCapacity);
                                asGroupsToManage.Add(asGroupToManage);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(ErrorStrings.ErrorLoadingAccount, accountKey.AccountName, ex.Message), ex);
                }
            }
            return asGroupsToManage.OrderBy(x => x.Name).ToList();
        }

        internal static async Task<UpdateAutoScalingGroupResponse> StartGroupAsync(IConfiguration Configuration, string User, string AccountName, string GroupName, int DesiredCapacity, int MaxCapacity, int MinCapacity)
        {
            try
            {
                AsgAwsAccountInfo accountKey = LoadAsgAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                AmazonSecurityTokenServiceClient stsClient = new();
                string sessionName = string.Format(ResourceStrings.StartAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                sessionName = sessionName.Length > 63 ? sessionName[..63] : sessionName;
                AssumeRoleRequest assumeRoleRequest = new()
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = sessionName,
                    DurationSeconds = 900
                };
                AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                UpdateAutoScalingGroupRequest updateAutoScalingGroupRequest = new() { AutoScalingGroupName = GroupName, DesiredCapacity = DesiredCapacity, MaxSize = MaxCapacity, MinSize = MinCapacity };
                AmazonAutoScalingClient asgClient = new(stsResponse.Credentials, accountRegion);
                Task<UpdateAutoScalingGroupResponse> response = asgClient.UpdateAutoScalingGroupAsync(updateAutoScalingGroupRequest);
                asgClient.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartEc2InstanceError, GroupName, e.Message), e.InnerException);
            }
        }

        internal static async Task<StartInstanceRefreshResponse> RefreshGroupAsync(IConfiguration Configuration, string User, string AccountName, string GroupName)
        {
            try
            {
                AsgAwsAccountInfo accountKey = LoadAsgAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                AmazonSecurityTokenServiceClient stsClient = new();
                string sessionName = string.Format(ResourceStrings.StartAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                sessionName = sessionName.Length > 63 ? sessionName[..63] : sessionName;
                AssumeRoleRequest assumeRoleRequest = new()
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = sessionName,
                    DurationSeconds = 900
                };
                AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                StartInstanceRefreshRequest instanceRefreshRequest = new() { AutoScalingGroupName = GroupName };
                AmazonAutoScalingClient asgClient = new(stsResponse.Credentials, accountRegion);
                StartInstanceRefreshResponse response = await asgClient.StartInstanceRefreshAsync(instanceRefreshRequest);
                asgClient.Dispose();
                stsClient.Dispose();
                return response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootEc2InstanceError, GroupName, e.Message), e.InnerException);
            }
        }

        internal static async Task<UpdateAutoScalingGroupResponse> StopGroupAsync(IConfiguration Configuration, string User, string AccountName, string GroupName)
        {
            try
            {
                AsgAwsAccountInfo accountKey = LoadAsgAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                AmazonSecurityTokenServiceClient stsClient = new();
                string sessionName = string.Format(ResourceStrings.StartAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                sessionName = sessionName.Length > 63 ? sessionName[..63] : sessionName;
                AssumeRoleRequest assumeRoleRequest = new()
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = sessionName,
                    DurationSeconds = 900
                };
                AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                UpdateAutoScalingGroupRequest updateAutoScalingGroupRequest = new() { AutoScalingGroupName = GroupName, DesiredCapacity = 0, MinSize = 0 };
                AmazonAutoScalingClient asgClient = new(stsResponse.Credentials, accountRegion);
                Task<UpdateAutoScalingGroupResponse> response = asgClient.UpdateAutoScalingGroupAsync(updateAutoScalingGroupRequest);
                asgClient.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartEc2InstanceError, GroupName, e.Message), e.InnerException);
            }
        }
    }
}
