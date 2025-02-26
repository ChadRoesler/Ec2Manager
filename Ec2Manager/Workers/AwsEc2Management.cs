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
    /// <summary>
    /// Provides methods for managing AWS EC2 instances.
    /// </summary>
    internal static class AwsEc2Management
    {
        /// <summary>
        /// Loads the AWS account information from the configuration.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <returns>A collection of AWS account information.</returns>
        internal static IEnumerable<Ec2AwsAccountInfo> LoadEc2AwsAccounts(IConfiguration Configuration)
        {
            return Configuration.GetSection("Ec2Manager:AwsAccounts").Get<IEnumerable<Ec2AwsAccountInfo>>();
        }

        /// <summary>
        /// Lists the EC2 instances for the specified user.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the list of instances.</param>
        /// <returns>A list of EC2 instances.</returns>
        internal static async Task<List<Ec2Instance>> ListEc2InstancesAsync(IConfiguration Configuration, string User)
        {
            List<Ec2Instance> ec2InstancesToManage = [];
            IEnumerable<Ec2AwsAccountInfo> accounts = LoadEc2AwsAccounts(Configuration);

            foreach (Ec2AwsAccountInfo accountKey in accounts)
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
                    using AmazonEC2Client ec2Client = new(stsResponse.Credentials, accountRegion);
                    DescribeInstancesResponse describeResponse = await ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest());

                    foreach (Reservation reservation in describeResponse.Reservations)
                    {
                        foreach (Instance instance in reservation.Instances)
                        {
                            var tag = instance.Tags.SingleOrDefault(t => t.Key == accountKey.TagToSearch);
                            if (tag != null && Regex.Match(tag.Value, accountKey.SearchString).Success)
                            {
                                string name = instance.Tags.SingleOrDefault(t => t.Key == accountKey.NameTag)?.Value;
                                string state = instance.State.Name.Value;

                                if (instance.State.Name == InstanceStateName.Running)
                                {
                                    DescribeInstanceStatusResponse statusResponse = await ec2Client.DescribeInstanceStatusAsync(new DescribeInstanceStatusRequest { InstanceIds = { instance.InstanceId } });
                                    var instanceStatus = statusResponse.InstanceStatuses.FirstOrDefault();
                                    if (instanceStatus != null)
                                    {
                                        if (string.Equals(instanceStatus.Status.Status.Value, "impaired", StringComparison.InvariantCultureIgnoreCase) ||
                                            string.Equals(instanceStatus.SystemStatus.Status.Value, "impaired", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            state = "impaired";
                                        }
                                        else if (string.Equals(instanceStatus.Status.Status.Value, "initializing", StringComparison.InvariantCultureIgnoreCase) ||
                                                 string.Equals(instanceStatus.SystemStatus.Status.Value, "initializing", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            state = "initializing";
                                        }
                                    }
                                }

                                Ec2Instance ec2InstanceToManage = new(name, instance.PrivateIpAddress, instance.InstanceId, state, accountKey.AccountName);
                                ec2InstancesToManage.Add(ec2InstanceToManage);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(ErrorStrings.ErrorLoadingAccount, accountKey.AccountName, ex.Message), ex);
                }
            }
            return [.. ec2InstancesToManage.OrderBy(x => x.Name)];
        }

        /// <summary>
        /// Starts the specified EC2 instance.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the start of the instance.</param>
        /// <param name="AccountName">The name of the AWS account.</param>
        /// <param name="InstanceId">The ID of the instance to start.</param>
        /// <returns>The response from the start instances request.</returns>
        internal static async Task<StartInstancesResponse> StartEc2InstanceAsync(IConfiguration Configuration, string User, string AccountName, string InstanceId)
        {
            try
            {
                Ec2AwsAccountInfo accountKey = LoadEc2AwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                List<string> instanceIdAsList = [InstanceId];
                StartInstancesRequest startRequest = new(instanceIdAsList);
                using AmazonEC2Client ec2Client = new(stsResponse.Credentials, accountRegion);
                return await ec2Client.StartInstancesAsync(startRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }

        /// <summary>
        /// Reboots the specified EC2 instance.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the reboot of the instance.</param>
        /// <param name="AccountName">The name of the AWS account.</param>
        /// <param name="InstanceId">The ID of the instance to reboot.</param>
        /// <returns>The response from the reboot instances request.</returns>
        internal static async Task<RebootInstancesResponse> RebootEc2InstanceAsync(IConfiguration Configuration, string User, string AccountName, string InstanceId)
        {
            try
            {
                Ec2AwsAccountInfo accountKey = LoadEc2AwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                using AmazonSecurityTokenServiceClient stsClient = new();
                string sessionName = string.Format(ResourceStrings.RebootAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                sessionName = sessionName.Length > 63 ? sessionName[..63] : sessionName;
                AssumeRoleRequest assumeRoleRequest = new()
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = sessionName,
                    DurationSeconds = 900
                };
                AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                List<string> instanceIdAsList = [InstanceId];
                RebootInstancesRequest rebootRequest = new(instanceIdAsList);
                using AmazonEC2Client ec2Client = new(stsResponse.Credentials, accountRegion);
                return await ec2Client.RebootInstancesAsync(rebootRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }

        /// <summary>
        /// Stops the specified EC2 instance.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the stop of the instance.</param>
        /// <param name="AccountName">The name of the AWS account.</param>
        /// <param name="InstanceId">The ID of the instance to stop.</param>
        /// <returns>The response from the stop instances request.</returns>
        internal static async Task<StopInstancesResponse> StopEc2InstanceAsync(IConfiguration Configuration, string User, string AccountName, string InstanceId)
        {
            try
            {
                Ec2AwsAccountInfo accountKey = LoadEc2AwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
                RegionEndpoint accountRegion = RegionEndpoint.GetBySystemName(accountKey.Region);
                using AmazonSecurityTokenServiceClient stsClient = new();
                string sessionName = string.Format(ResourceStrings.StopAction, User, accountKey.AccountName, DateTime.Now.Ticks.ToString());
                sessionName = sessionName.Length > 63 ? sessionName[..63] : sessionName;
                AssumeRoleRequest assumeRoleRequest = new()
                {
                    RoleArn = accountKey.RoleArn,
                    RoleSessionName = sessionName,
                    DurationSeconds = 900
                };
                AssumeRoleResponse stsResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                List<string> instanceIdAsList = [InstanceId];
                StopInstancesRequest stopRequest = new(instanceIdAsList);
                using AmazonEC2Client ec2Client = new(stsResponse.Credentials, accountRegion);
                return await ec2Client.StopInstancesAsync(stopRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StopEc2InstanceError, InstanceId, e.Message), e.InnerException);
            }
        }
    }
}
