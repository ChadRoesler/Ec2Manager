namespace Ec2Manager.Constants
{
    internal class MessageStrings
    {
        internal const string UserEnable = "User: {0} has initiated starting Instance Id: {1}";
        internal const string UserReboot = "User: {0} has initiated rebooting Instance Id: {1}";
        internal const string LoadingInstances = "User: {0} called initial instance load";
        internal const string InitialInstanceCount = "Found: {0} instances";
        internal const string SearchedInstanceCount = "Found: {0} instances with the following parameters: [searchtype:{1}][query:{2}][sortorder:{3}]";
        internal const string UserEnableSuccess = "User: {0} successfully enabled Instance Id: {1}";
        internal const string UserRebootSuccess = "User: {0} successfully rebooted Instance Id: {1}";
    }
}
