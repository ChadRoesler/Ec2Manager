namespace Ec2Manager.Constants
{
    /// <summary>
    /// Contains constant message strings used for logging and user notifications.
    /// </summary>
    internal static class MessageStrings
    {
        /// <summary>
        /// Message indicating user account management details.
        /// </summary>
        internal const string UserAccountManagment = "User: {0} has access to the following accounts: {1}";

        /// <summary>
        /// Message indicating the initiation of starting an EC2 instance.
        /// </summary>
        internal const string UserEc2Enable = "User: {0} has initiated starting Ec2 Instance Id: {1}";

        /// <summary>
        /// Message indicating the initiation of rebooting an EC2 instance.
        /// </summary>
        internal const string UserEc2Reboot = "User: {0} has initiated rebooting Ec2 Instance Id: {1}";

        /// <summary>
        /// Message indicating the initiation of stopping an EC2 instance.
        /// </summary>
        internal const string UserEc2Stop = "User: {0} has initiated stopping Ec2 Instance Id: {1}";

        /// <summary>
        /// Message indicating the initial load of EC2 instances.
        /// </summary>
        internal const string LoadingEc2Instances = "User: {0} called initial Ec2 Instance load";

        /// <summary>
        /// Message indicating the count of initially loaded EC2 instances.
        /// </summary>
        internal const string InitialEc2InstanceCount = "Found: {0} Ec2 Instances";

        /// <summary>
        /// Message indicating the count of EC2 instances found with specific search parameters.
        /// </summary>
        internal const string SearchedEc2InstanceCount = "Found: {0} Ec2 Instances with the following parameters: [searchtype:{1}][query:{2}][sortorder:{3}]";

        /// <summary>
        /// Message indicating the successful enabling of an EC2 instance.
        /// </summary>
        internal const string UserEc2EnableSuccess = "User: {0} successfully enabled Ec2 Instance Id: {1} with HttpStatusCode: {2}";

        /// <summary>
        /// Message indicating the successful rebooting of an EC2 instance.
        /// </summary>
        internal const string UserEc2RebootSuccess = "User: {0} successfully rebooted Ec2 Instance Id: {1} with HttpStatusCode: {2}";

        /// <summary>
        /// Message indicating the successful stopping of an EC2 instance.
        /// </summary>
        internal const string UserEc2StopSuccess = "User: {0} successfully stopped Ec2 Instance Id: {1} with HttpStatusCode: {2}";

        /// <summary>
        /// Message indicating the initiation of starting an RDS instance.
        /// </summary>
        internal const string UserRdsEnable = "User: {0} has initiated starting Rds Instance Id: {1}";

        /// <summary>
        /// Message indicating the initiation of rebooting an RDS instance.
        /// </summary>
        internal const string UserRdsReboot = "User: {0} has initiated rebooting Rds Instance Id: {1}";

        /// <summary>
        /// Message indicating the initiation of stopping an RDS instance.
        /// </summary>
        internal const string UserRdsStop = "User: {0} has initiated stopping Rds Instance Id: {1}";

        /// <summary>
        /// Message indicating the initial load of RDS instances.
        /// </summary>
        internal const string LoadingRdsInstances = "User: {0} called initial Rds Instance load";

        /// <summary>
        /// Message indicating the count of initially loaded RDS instances.
        /// </summary>
        internal const string InitialRdsInstanceCount = "Found: {0} Rds Instances";

        /// <summary>
        /// Message indicating the count of RDS instances found with specific search parameters.
        /// </summary>
        internal const string SearchedRdsInstanceCount = "Found: {0} Rds Instances with the following parameters: [searchtype:{1}][query:{2}][sortorder:{3}]";

        /// <summary>
        /// Message indicating the successful enabling of an RDS instance.
        /// </summary>
        internal const string UserRdsEnableSuccess = "User: {0} successfully enabled Rds Instance Id: {1} with HttpStatusCode: {2}";

        /// <summary>
        /// Message indicating the successful rebooting of an RDS instance.
        /// </summary>
        internal const string UserRdsRebootSuccess = "User: {0} successfully rebooted Rds Instance Id: {1} with HttpStatusCode: {2}";

        /// <summary>
        /// Message indicating the successful stopping of an RDS instance.
        /// </summary>
        internal const string UserRdsStopSuccess = "User: {0} successfully stopped Rds Instance Id: {1} with HttpStatusCode: {2}";

        /// <summary>
        /// Message indicating the initiation of starting an ASG group.
        /// </summary>
        internal const string UserAsgEnable = "User: {0} has initiated starting Asg Group Name: {1}";

        /// <summary>
        /// Message indicating the initiation of rebooting an ASG group.
        /// </summary>
        internal const string UserAsgRefresh = "User: {0} has initiated rebooting Asg Group Name: {1}";

        /// <summary>
        /// Message indicating the initiation of stopping an ASG group.
        /// </summary>
        internal const string UserAsgStop = "User: {0} has initiated stopping Asg Group Name: {1}";

        /// <summary>
        /// Message indicating the initial load of ASG groups.
        /// </summary>
        internal const string LoadingAsGroups = "User: {0} called initial As Group load";

        /// <summary>
        /// Message indicating the count of initially loaded ASG groups.
        /// </summary>
        internal const string InitialAsGroupCount = "Found: {0} As Groups";

        /// <summary>
        /// Message indicating the count of ASG groups found with specific search parameters.
        /// </summary>
        internal const string SearchedAsGroupCount = "Found: {0} As Groups with the following parameters: [searchtype:{1}][query:{2}][sortorder:{3}]";

        /// <summary>
        /// Message indicating the successful enabling of an ASG group.
        /// </summary>
        internal const string UserAsgEnableSuccess = "User: {0} successfully enabled Asg Group Name: {1} with HttpStatusCode: {2}";

        /// <summary>
        /// Message indicating the successful refreshing of an ASG group.
        /// </summary>
        internal const string UserAsgRefreshSuccess = "User: {0} successfully refreshed Asg Group Name: {1} with HttpStatusCode: {2}";

        /// <summary>
        /// Message indicating the successful stopping of an ASG group.
        /// </summary>
        internal const string UserAsgStopSuccess = "User: {0} successfully stopped Asg Group Name: {1} with HttpStatusCode: {2}";
    }
}
