namespace Ec2Manager.Interfaces
{
    /// <summary>
    /// Interface representing AWS account information.
    /// </summary>
    interface IAwsAccountInfo
    {
        /// <summary>
        /// Gets the account name.
        /// </summary>
        string AccountName { get; init; }

        /// <summary>
        /// Gets the role ARN.
        /// </summary>
        string RoleArn { get; init; }

        /// <summary>
        /// Gets the region.
        /// </summary>
        string Region { get; init; }

        /// <summary>
        /// Gets the tag to search.
        /// </summary>
        string TagToSearch { get; init; }

        /// <summary>
        /// Gets the search string.
        /// </summary>
        string SearchString { get; init; }
    }
}
