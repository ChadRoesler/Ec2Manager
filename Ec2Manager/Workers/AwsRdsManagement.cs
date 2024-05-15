using Amazon;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Ec2Manager.Constants;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;
using Amazon.RDS;
using Amazon.RDS.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ec2Manager.Workers
{
    public static class AwsRdsManagement
    {

        internal static IEnumerable<RdsAwsAccountInfo> LoadRdsAwsAccounts(IConfiguration Configuration)
        {
            IEnumerable<RdsAwsAccountInfo> awsKeys = Configuration.GetSection("RdsManager:AwsAccounts").Get<IEnumerable<RdsAwsAccountInfo>>();
            return awsKeys;
        }
        internal static async Task<List<RdsInstance>> ListRdsInstancesAsync(IConfiguration Configuration, string User)
        {
            List<RdsInstance> rdsInstancesToManage = new();
            IEnumerable<RdsAwsAccountInfo> accounts = LoadRdsAwsAccounts(Configuration);

            foreach (RdsAwsAccountInfo accountKey in accounts)
            {
                try
                {
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
                    AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
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
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
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
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
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
                RdsAwsAccountInfo accountKey = LoadRdsAwsAccounts(Configuration).SingleOrDefault(x => x.AccountName == AccountName);
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
                AmazonRDSClient rdsClient = new(stsResponse.Credentials, RegionEndpoint.GetBySystemName(accountKey.Region));
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

    }
}
