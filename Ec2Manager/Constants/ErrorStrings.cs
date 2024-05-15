namespace Ec2Manager.Constants
{
    internal class ErrorStrings
    {
        internal const string StartEc2InstanceError = "Unable to Start Ec2 Instance: {0}.  The following error occured: {1}";
        internal const string RebootEc2InstanceError = "Unable to Reboot Ec2 Instance {0}.  The following error occured: {1}";
        internal const string StopEc2InstanceError = "Unable to Stop Ec2 Instance {0}.  The following error occured: {1}";
        internal const string StartRdsInstanceError = "Unable to Start Rds Instance: {0}.  The following error occured: {1}";
        internal const string RebootRdsInstanceError = "Unable to Reboot Rds Instance {0}.  The following error occured: {1}";
        internal const string StopRdsInstanceError = "Unable to Stop Rds Instance {0}.  The following error occured: {1}";
        internal const string ErrorLoadingAccount = "Unable to load Account: {0}.  The following error occured: {1}";
        internal const string ErrorLoadingSecret = "Unable to load Secret: {0}.  The following error occured: {1}";
    }
}
