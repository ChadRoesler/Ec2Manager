using Ec2Manager.Interfaces;

namespace Ec2Manager.Models.ConfigManagement
{
    /// <summary>
    /// Represents AWS account information for RDS.
    /// </summary>
    public class RdsAwsAccountInfo : IAwsAccountInfo
    {
        /// <summary>
        /// Gets the account name.
        /// </summary>
        public string AccountName { get; init; }

        /// <summary>
        /// Gets the role ARN.
        /// </summary>
        public string RoleArn { get; init; }

        /// <summary>
        /// Gets the region.
        /// </summary>
        public string Region { get; init; }

        /// <summary>
        /// Gets the tag to search.
        /// </summary>
        public string TagToSearch { get; init; }

        /// <summary>
        /// Gets the search string.
        /// </summary>
        public string SearchString { get; init; }
    }
}
