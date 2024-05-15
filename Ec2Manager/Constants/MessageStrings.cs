namespace Ec2Manager.Constants
{
    internal class MessageStrings
    {
        internal const string UserAccountManagment = "User: {0} has access to the following accounts: {1}";
        internal const string UseEc2rEnable = "User: {0} has initiated starting Ec2 Instance Id: {1}";
        internal const string UserEc2Reboot = "User: {0} has initiated rebooting Ec2 Instance Id: {1}";
        internal const string UserEc2Stop = "User: {0} has initiated stopping Ec2 Instance Id: {1}";
        internal const string LoadingEc2Instances = "User: {0} called initial Ec2 Instance load";
        internal const string InitialEc2InstanceCount = "Found: {0} Ec2 Instances";
        internal const string SearchedEc2InstanceCount = "Found: {0} Ec2 Instances with the following parameters: [searchtype:{1}][query:{2}][sortorder:{3}]";
        internal const string UserEc2EnableSuccess = "User: {0} successfully enabled Ec2 Instance Id: {1} with HttpStatusCode: {2}";
        internal const string UserEc2RebootSuccess = "User: {0} successfully rebooted Ec2 Instance Id: {1} with HttpStatusCode: {2}";
        internal const string UserEc2StopSuccess = "User: {0} successfully stopped Ec2 Instance Id: {1} with HttpStatusCode: {2}";
        internal const string UseRdsrEnable = "User: {0} has initiated starting Rds Instance Id: {1}";
        internal const string UserRdsReboot = "User: {0} has initiated rebooting Rds Instance Id: {1}";
        internal const string UserRdsStop = "User: {0} has initiated stopping Rds Instance Id: {1}";
        internal const string LoadingRdsInstances = "User: {0} called initial Rds Instance load";
        internal const string InitialRdsInstanceCount = "Found: {0} Rds Instances";
        internal const string SearchedRdsInstanceCount = "Found: {0} Rds Instances with the following parameters: [searchtype:{1}][query:{2}][sortorder:{3}]";
        internal const string UserRdsEnableSuccess = "User: {0} successfully enabled Rds Instance Id: {1} with HttpStatusCode: {2}";
        internal const string UserRdsRebootSuccess = "User: {0} successfully rebooted Rds Instance Id: {1} with HttpStatusCode: {2}";
        internal const string UserRdsStopSuccess = "User: {0} successfully stopped Rds Instance Id: {1} with HttpStatusCode: {2}";
    }
}
