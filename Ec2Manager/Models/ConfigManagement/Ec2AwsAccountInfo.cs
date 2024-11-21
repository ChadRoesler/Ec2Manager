namespace Ec2Manager.Models.ConfigManagement
{
    /// <summary>
    /// Represents AWS account information for EC2.
    /// </summary>
    public class Ec2AwsAccountInfo
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
        /// Gets the name tag.
        /// </summary>
        public string NameTag { get; init; }
    }
}
