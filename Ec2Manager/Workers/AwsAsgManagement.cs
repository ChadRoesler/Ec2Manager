using Amazon;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Ec2Manager.Constants;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;
using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.RDS.Model;

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
                    sessionName = sessionName.Length > 63 ? sessionName.Substring(0, 63) : sessionName;
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
                                AssumeRoleResponse stsResponseIr = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                                DescribeInstanceRefreshesRequest describeIrRequest = new DescribeInstanceRefreshesRequest() { AutoScalingGroupName = group.AutoScalingGroupName };
                                AmazonAutoScalingClient asgClientIr = new(stsResponseIr.Credentials, accountRegion);
                                DescribeInstanceRefreshesResponse describeResponseIr = await asgClient.DescribeInstanceRefreshesAsync(describeIrRequest);
                                AsGroup asGroupToManage = new(group.AutoScalingGroupName, group.Instances.Count(), (describeResponseIr.InstanceRefreshes.OrderBy( x => x.StartTime).ToArray()[0].Status == InstanceRefreshStatus.InProgress), accountKey.AccountName);
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

        internal static async Task<StartInstancesResponse> StartEc2InstanceAsync(IConfiguration Configuration, string User, string AccountName, string InstanceId)
        {
            try
            {
                AsgAwsAccountInfo accountKey = LoadAsgAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                Ec2AwsAccountInfo accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                Ec2AwsAccountInfo accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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

        internal static async Task<List<RdsInstance>> ListRdsInstancesAsync(IConfiguration Configuration, string User)
        {
            List<RdsInstance> rdsInstancesToManage = new();
            IEnumerable<Ec2AwsAccountInfo> accounts = LoadAwsAccounts(Configuration);

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
                    DescribeDBInstancesRequest describeRequest = new();
                    AmazonRDSClient rdsClient = new(stsResponse.Credentials, accountRegion);
                    var describeResponse = await rdsClient.DescribeDBInstancesAsync(describeRequest);
                    rdsClient.Dispose();
                    stsClient.Dispose();
                    foreach (DBInstance dbInstance in describeResponse.DBInstances)
                    {
                        if (dbInstance.TagList.Where(t => t.Key == accountKey.TagToSearch).FirstOrDefault() != null)
                        {
                            if (Regex.Match(dbInstance.TagList.SingleOrDefault(t => t.Key == accountKey.TagToSearch)?.Value, accountKey.SearchString).Success)
                            {
                                RdsInstance rdsInstanceToManage = new(dbInstance.DBInstanceIdentifier, dbInstance.Endpoint.ToString().Replace("rds.amazonaws.com",string.Empty), dbInstance.DBInstanceStatus, accountKey.AccountName);
                                rdsInstancesToManage.Add(rdsInstanceToManage);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(ErrorStrings.ErrorLoadingAccount, accountKey.AccountName, ex.Message), ex);
                }
            }
            return rdsInstancesToManage.OrderBy(x => x.DbIdentifier).ToList();
        }

        internal static async Task<StartDBInstanceResponse> StartRdsInstanceAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                Ec2AwsAccountInfo accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                StartDBInstanceRequest startRequest = new() { DBInstanceIdentifier = DbIdentifier };
                AmazonRDSClient rdsClient = new(stsResponse.Credentials, accountRegion);
                Task<StartDBInstanceResponse> response = rdsClient.StartDBInstanceAsync(startRequest);
                rdsClient.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }

        internal static async Task<RebootDBInstanceResponse> RebootRdsInstanceAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                Ec2AwsAccountInfo accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                RebootDBInstanceRequest rebootRequest = new(DbIdentifier);
                AmazonRDSClient rdsClient = new(stsResponse.Credentials, accountRegion);
                Task<RebootDBInstanceResponse> response = rdsClient.RebootDBInstanceAsync(rebootRequest);
                rdsClient.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }

        internal static async Task<StopDBInstanceResponse> StopRdsInstanceAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                Ec2AwsAccountInfo accountKey = LoadAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                StopDBInstanceRequest stopRequest = new() { DBInstanceIdentifier = DbIdentifier };
                AmazonRDSClient rdsClient = new(stsResponse.Credentials, accountRegion);
                Task<StopDBInstanceResponse> response = rdsClient.StopDBInstanceAsync(stopRequest);
                rdsClient.Dispose();
                stsClient.Dispose();
                return await response;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StopRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }

        internal static async Task<> ListAsGroupsAsync(IConfiguration Configuration, string User)
        {

        }
    }
}
