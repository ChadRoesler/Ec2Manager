namespace Ec2Manager.Constants
{
    /// <summary>
    /// Contains constant error message strings used for logging and user notifications.
    /// </summary>
    internal static class ErrorStrings
    {
        /// <summary>
        /// Error message for failing to start an EC2 instance.
        /// </summary>
        internal const string StartEc2InstanceError = "Unable to Start Ec2 Instance: {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to reboot an EC2 instance.
        /// </summary>
        internal const string RebootEc2InstanceError = "Unable to Reboot Ec2 Instance {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to stop an EC2 instance.
        /// </summary>
        internal const string StopEc2InstanceError = "Unable to Stop Ec2 Instance {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to start an RDS instance.
        /// </summary>
        internal const string StartRdsInstanceError = "Unable to Start Rds Instance: {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to reboot an RDS instance.
        /// </summary>
        internal const string RebootRdsInstanceError = "Unable to Reboot Rds Instance {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to stop an RDS instance.
        /// </summary>
        internal const string StopRdsInstanceError = "Unable to Stop Rds Instance {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to start an AS group.
        /// </summary>
        internal const string StartAsGroupError = "Unable to Start As Group: {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to refresh an AS group.
        /// </summary>
        internal const string RefreshAsGroupError = "Unable to Refresh As Group {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to stop an AS group.
        /// </summary>
        internal const string StopAsGroupError = "Unable to Stop As Group {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to load an account.
        /// </summary>
        internal const string ErrorLoadingAccount = "Unable to load Account: {0}. The following error occurred: {1}";

        /// <summary>
        /// Error message for failing to load a secret.
        /// </summary>
        internal const string ErrorLoadingSecret = "Unable to load Secret: {0}. The following error occurred: {1}";
    }
}
