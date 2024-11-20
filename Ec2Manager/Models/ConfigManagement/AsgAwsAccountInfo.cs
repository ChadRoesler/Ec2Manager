namespace Ec2Manager.Models.ConfigManagement
{
    /// <summary>
    /// Represents AWS account information for Auto Scaling Groups (ASG).
    /// </summary>
    public class AsgAwsAccountInfo
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

        /// <summary>
        /// Gets the desired capacity tag.
        /// </summary>
        public string DesiredCapacityTag { get; init; }

        /// <summary>
        /// Gets the minimum capacity tag.
        /// </summary>
        public string MinCapacityTag { get; init; }

        /// <summary>
        /// Gets the maximum capacity tag.
        /// </summary>
        public string MaxCapacityTag { get; init; }
    }
}
