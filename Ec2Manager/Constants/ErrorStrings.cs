﻿namespace Ec2Manager.Constants
{
    internal class ErrorStrings
    {
        internal const string StartEc2InstanceError = "Unable to Start Instance: {0}.  The following error occured: {1}";
        internal const string RebootEc2InstanceError = "Unable to Reboot Instance {0}.  The following error occured: {1}";
        internal const string ErrorLoadingAccount = "Unable to load Account: {0}.  The following error occured: {1}";
        internal const string ErrorLoadingSecret = "Unable to load Secret: {0}.  The following error occured: {1}";
    }
}
