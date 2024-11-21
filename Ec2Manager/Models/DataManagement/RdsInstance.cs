using System.ComponentModel.DataAnnotations;

namespace Ec2Manager.Models.DataManagement
{
    /// <summary>
    /// Represents an RDS instance in AWS.
    /// </summary>
    public class RdsInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RdsInstance"/> class.
        /// </summary>
        /// <param name="dbIdentifier">The database identifier of the RDS instance.</param>
        /// <param name="endpoint">The endpoint of the RDS instance.</param>
        /// <param name="status">The status of the RDS instance.</param>
        /// <param name="accountName">The name of the AWS account.</param>
        public RdsInstance(string dbIdentifier, string endpoint, string status, string accountName)
        {
            DbIdentifier = dbIdentifier;
            Endpoint = endpoint;
            Status = status;
            Account = accountName;
        }

        /// <summary>
        /// Gets the status image based on the current status.
        /// </summary>
        public string StatusImage => Status.ToLowerInvariant() switch
        {
            "stopped" => "images/stopped.png",
            "running" => "images/started.png",
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
        /// Gets or sets the database identifier of the RDS instance.
        /// </summary>
        [Display(Name = "Name")]
        public string DbIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the endpoint of the RDS instance.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the status of the RDS instance.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RDS instance can be rebooted.
        /// </summary>
        public bool CanReboot { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the RDS instance can be stopped.
        /// </summary>
        public bool CanStop { get; set; } = false;
    }
}
