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
    internal static class AwsAsgManagement
    {
        /// <summary>
        /// Loads the Auto Scaling Group AWS accounts from the configuration.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <returns>A collection of AsgAwsAccountInfo objects.</returns>
        internal static IEnumerable<AsgAwsAccountInfo> LoadAsgAwsAccounts(IConfiguration Configuration)
        {
            return Configuration.GetSection("AsgManager:AwsAccounts").Get<IEnumerable<AsgAwsAccountInfo>>();
        }

        /// <summary>
        /// Lists the Auto Scaling Groups asynchronously.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the list.</param>
        /// <returns>A list of AsGroup objects.</returns>
        internal static async Task<List<AsGroup>> ListAsGroupsAsync(IConfiguration Configuration, string User)
        {
            List<AsGroup> asGroupsToManage = [];
            IEnumerable<AsgAwsAccountInfo> accounts = LoadAsgAwsAccounts(Configuration);

            foreach (AsgAwsAccountInfo accountKey in accounts)
            {
                try
                {
                    RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                    using AmazonSecurityTokenServiceClient stsClient = new();
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
                    using AmazonAutoScalingClient asgClient = new(stsResponse.Credentials, accountRegion);
                    DescribeAutoScalingGroupsResponse describeResponse = await asgClient.DescribeAutoScalingGroupsAsync(describeRequest);

                    foreach (AutoScalingGroup group in describeResponse.AutoScalingGroups)
                    {
                        var tag = group.Tags.SingleOrDefault(t => t.Key == accountKey.TagToSearch);
                        if (tag != null && Regex.Match(tag.Value, accountKey.SearchString).Success)
                        {
                            int.TryParse(group.Tags.SingleOrDefault(t => t.Key == accountKey.DesiredCapacityTag)?.Value ?? "1", out int desiredCapacity);
                            int.TryParse(group.Tags.SingleOrDefault(t => t.Key == accountKey.MaxCapacityTag)?.Value ?? "1", out int maxCapacity);
                            int.TryParse(group.Tags.SingleOrDefault(t => t.Key == accountKey.MinCapacityTag)?.Value ?? "1", out int minCapacity);

                            DescribeInstanceRefreshesRequest describeIrRequest = new() { AutoScalingGroupName = group.AutoScalingGroupName };
                            DescribeInstanceRefreshesResponse describeResponseIr = await asgClient.DescribeInstanceRefreshesAsync(describeIrRequest);
                            bool refreshStatus = describeResponseIr.InstanceRefreshes.OrderBy(x => x.StartTime).FirstOrDefault()?.Status == InstanceRefreshStatus.InProgress;

                            AsGroup asGroupToManage = new(group.AutoScalingGroupName, group.Instances.Count, group.DesiredCapacity, refreshStatus, accountKey.AccountName, desiredCapacity, maxCapacity, minCapacity);
                            asGroupsToManage.Add(asGroupToManage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(ErrorStrings.ErrorLoadingAccount, accountKey.AccountName, ex.Message), ex);
                }
            }
            return [.. asGroupsToManage.OrderBy(x => x.Name)];
        }

        /// <summary>
        /// Starts the specified Auto Scaling Group asynchronously.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the start.</param>
        /// <param name="AccountName">The name of the account.</param>
        /// <param name="GroupName">The name of the group.</param>
        /// <param name="DesiredCapacity">The desired capacity.</param>
        /// <param name="MaxCapacity">The maximum capacity.</param>
        /// <param name="MinCapacity">The minimum capacity.</param>
        /// <returns>The response from the UpdateAutoScalingGroup operation.</returns>
        internal static async Task<UpdateAutoScalingGroupResponse> StartGroupAsync(IConfiguration Configuration, string User, string AccountName, string GroupName, int DesiredCapacity, int MaxCapacity, int MinCapacity)
        {
            try
            {
                AsgAwsAccountInfo accountKey = LoadAsgAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                using AmazonSecurityTokenServiceClient stsClient = new();
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
                using AmazonAutoScalingClient asgClient = new(stsResponse.Credentials, accountRegion);
                return await asgClient.UpdateAutoScalingGroupAsync(updateAutoScalingGroupRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartEc2InstanceError, GroupName, e.Message), e.InnerException);
            }
        }

        /// <summary>
        /// Refreshes the specified Auto Scaling Group asynchronously.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the refresh.</param>
        /// <param name="AccountName">The name of the account.</param>
        /// <param name="GroupName">The name of the group.</param>
        /// <returns>The response from the StartInstanceRefresh operation.</returns>
        internal static async Task<StartInstanceRefreshResponse> RefreshGroupAsync(IConfiguration Configuration, string User, string AccountName, string GroupName)
        {
            try
            {
                AsgAwsAccountInfo accountKey = LoadAsgAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                using AmazonSecurityTokenServiceClient stsClient = new();
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
                using AmazonAutoScalingClient asgClient = new(stsResponse.Credentials, accountRegion);
                return await asgClient.StartInstanceRefreshAsync(instanceRefreshRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootEc2InstanceError, GroupName, e.Message), e.InnerException);
            }
        }

        /// <summary>
        /// Stops the specified Auto Scaling Group asynchronously.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the stop.</param>
        /// <param name="AccountName">The name of the account.</param>
        /// <param name="GroupName">The name of the group.</param>
        /// <returns>The response from the UpdateAutoScalingGroup operation.</returns>
        internal static async Task<UpdateAutoScalingGroupResponse> StopGroupAsync(IConfiguration Configuration, string User, string AccountName, string GroupName)
        {
            try
            {
                AsgAwsAccountInfo accountKey = LoadAsgAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                using AmazonSecurityTokenServiceClient stsClient = new();
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
                using AmazonAutoScalingClient asgClient = new(stsResponse.Credentials, accountRegion);
                return await asgClient.UpdateAutoScalingGroupAsync(updateAutoScalingGroupRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartEc2InstanceError, GroupName, e.Message), e.InnerException);
            }
        }
    }
}
