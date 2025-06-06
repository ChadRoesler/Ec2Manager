﻿using Amazon;
using Amazon.RDS;
using Amazon.RDS.Model;
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
    /// Provides methods for managing AWS RDS instances.
    /// </summary>
    internal static class AwsRdsManagement
    {
        /// <summary>
        /// Loads the AWS account information for RDS from the configuration.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <returns>A collection of AWS account information for RDS.</returns>
        internal static IEnumerable<RdsAwsAccountInfo> LoadRdsAwsAccounts(IConfiguration Configuration)
        {
            IEnumerable<RdsAwsAccountInfo> awsKeys = Configuration.GetSection("RdsManager:AwsAccounts").Get<IEnumerable<RdsAwsAccountInfo>>();
            return awsKeys;
        }

        /// <summary>
        /// Lists the RDS instances for the specified user.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the list of instances.</param>
        /// <returns>A list of RDS instances.</returns>
        internal static async Task<List<RdsObject>> ListRdsInstancesAsync(IConfiguration Configuration, string User)
        {
            List<RdsObject> rdsObjectsToManage = [];
            IEnumerable<RdsAwsAccountInfo> accounts = LoadRdsAwsAccounts(Configuration);

            foreach (RdsAwsAccountInfo accountKey in accounts)
            {
                try
                {
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
                    using AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
                    var describeResponse = await rdsClient.DescribeDBInstancesAsync(new DescribeDBInstancesRequest());

                    foreach (DBInstance dbInstance in describeResponse.DBInstances)
                    {
                        var tag = dbInstance.TagList.SingleOrDefault(t => t.Key == accountKey.TagToSearch);
                        if (tag != null && Regex.Match(tag.Value, accountKey.SearchString).Success)
                        {
                            RdsObject rdsInstanceToManage = new(dbInstance.DBInstanceIdentifier, $"{dbInstance.Endpoint.Address.Replace($".{accountKey.Region}.rds.amazonaws.com", string.Empty)}:{dbInstance.Endpoint.Port}", dbInstance.DBInstanceStatus, accountKey.AccountName, false);
                            rdsObjectsToManage.Add(rdsInstanceToManage);
                        }
                    }
                    var describeResponse2 = await rdsClient.DescribeDBClustersAsync(new DescribeDBClustersRequest());

                    foreach (DBCluster dbCluster in describeResponse2.DBClusters)
                    {
                        var tag = dbCluster.TagList.SingleOrDefault(t => t.Key == accountKey.TagToSearch);
                        if (tag != null && Regex.Match(tag.Value, accountKey.SearchString).Success)
                        {
                            RdsObject rdsClusterToManage = new(dbCluster.DBClusterIdentifier, $"{dbCluster.Endpoint.Replace($".{accountKey.Region}.rds.amazonaws.com", string.Empty)}:{dbCluster.Port}", dbCluster.Status, accountKey.AccountName, true);
                            rdsObjectsToManage.Add(rdsClusterToManage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(ErrorStrings.ErrorLoadingAccount, accountKey.AccountName, ex.Message), ex);
                }
            }
            return [.. rdsObjectsToManage.OrderBy(x => x.DbIdentifier)];
        }

        /// <summary>
        /// Starts the specified RDS instance.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the start of the instance.</param>
        /// <param name="AccountName">The name of the AWS account.</param>
        /// <param name="DbIdentifier">The identifier of the RDS instance to start.</param>
        /// <returns>The response from the start RDS instance request.</returns>
        internal static async Task<StartDBInstanceResponse> StartRdsInstanceAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                using AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
                StartDBInstanceRequest startRequest = new() { DBInstanceIdentifier = DbIdentifier };
                return await rdsClient.StartDBInstanceAsync(startRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }

        internal static async Task<StartDBClusterResponse> StartRdsClusterAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                using AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
                StartDBClusterRequest startRequest = new() { DBClusterIdentifier = DbIdentifier };
                return await rdsClient.StartDBClusterAsync(startRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StartRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }

        /// <summary>
        /// Reboots the specified RDS instance.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the reboot of the instance.</param>
        /// <param name="AccountName">The name of the AWS account.</param>
        /// <param name="DbIdentifier">The identifier of the RDS instance to reboot.</param>
        /// <returns>The response from the reboot RDS instance request.</returns>
        internal static async Task<RebootDBInstanceResponse> RebootRdsInstanceAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                using AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
                RebootDBInstanceRequest rebootRequest = new(DbIdentifier);
                return await rdsClient.RebootDBInstanceAsync(rebootRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }

        internal static async Task<RebootDBClusterResponse> RebootRdsClusterAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                using AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
                RebootDBClusterRequest rebootRequest = new() { DBClusterIdentifier = DbIdentifier };
                return await rdsClient.RebootDBClusterAsync(rebootRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.RebootRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }

        /// <summary>
        /// Stops the specified RDS instance.
        /// </summary>
        /// <param name="Configuration">The configuration object.</param>
        /// <param name="User">The user requesting the stop of the instance.</param>
        /// <param name="AccountName">The name of the AWS account.</param>
        /// <param name="DbIdentifier">The identifier of the RDS instance to stop.</param>
        /// <returns>The response from the stop RDS instance request.</returns>
        internal static async Task<StopDBInstanceResponse> StopRdsInstanceAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                using AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
                StopDBInstanceRequest stopInstanceRequest = new() { DBInstanceIdentifier = DbIdentifier };
                return await rdsClient.StopDBInstanceAsync(stopInstanceRequest);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StopRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }

        internal static async Task<StopDBClusterResponse> StopRdsClusterAsync(IConfiguration Configuration, string User, string AccountName, string DbIdentifier)
        {
            try
            {
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                using AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
                StopDBClusterRequest stopClusterRequest = new() { DBClusterIdentifier = DbIdentifier };
                return await rdsClient.StopDBClusterAsync(stopClusterRequest);

            }
            catch (Exception e)
            {
                throw new Exception(string.Format(ErrorStrings.StopRdsInstanceError, DbIdentifier, e.Message), e.InnerException);
            }
        }
    }
}
