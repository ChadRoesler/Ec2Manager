using System.ComponentModel.DataAnnotations;

namespace Ec2Manager.Models.DataManagement
{
    /// <summary>
    /// Represents an RDS instance in AWS.
    /// </summary>
    public class RdsObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RdsObject"/> class.
        /// </summary>
        /// <param name="dbIdentifier">The database identifier of the RDS instance.</param>
        /// <param name="endpoint">The endpoint of the RDS instance.</param>
        /// <param name="status">The status of the RDS instance.</param>
        /// <param name="accountName">The name of the AWS account.</param>
        public RdsObject(string dbIdentifier, string endpoint, string status, string accountName, bool isCluster)
        {
            DbIdentifier = dbIdentifier;
            Endpoint = endpoint;
            Status = status;
            Account = accountName;
            IsCluster = isCluster;
        }

        /// <summary>
        /// Gets the status image based on the current status.
        /// </summary>
        public string StatusImage => Status.ToLowerInvariant() switch
        {
            "stopped" => "images/stopped.png",
            "running" => "images/started.png",
            "available" => "images/started.png",
            "initializing" => "images/changing.png",
            "starting" => "images/started.png",
            "stopping" => "images/stopped.png",
            "shutting-down" => "images/changing.png",
            "terminated" => "images/unknown.png",
            "impaired" => "images/warning.png",
            _ => "images/unknown.png"
        };

        /// <summary>
        /// Gets or sets the AWS account name.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the database identifier of the RDS object.
        /// </summary>
        [Display(Name = "Name")]
        public string DbIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the endpoint of the RDS object.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the status of the RDS object.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RDS object is a cluster.
        /// </summary>
        [Display(Name = "Is Cluster")]
        public bool IsCluster { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RDS object can be rebooted.
        /// </summary>
        public bool CanReboot { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the RDS instaobjectnce can be stopped.
        /// </summary>
        public bool CanStop { get; set; } = false;
    }
}
