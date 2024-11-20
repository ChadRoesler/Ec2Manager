using Ec2Manager.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ec2Manager.Models.DataManagement
{
    /// <summary>
    /// Represents an EC2 instance in AWS.
    /// </summary>
    public class Ec2Instance : IAwsResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ec2Instance"/> class.
        /// </summary>
        /// <param name="name">The name of the EC2 instance.</param>
        /// <param name="ipAddress">The IP address of the EC2 instance.</param>
        /// <param name="id">The ID of the EC2 instance.</param>
        /// <param name="status">The status of the EC2 instance.</param>
        /// <param name="accountName">The name of the AWS account.</param>
        public Ec2Instance(string name, string ipAddress, string id, string status, string accountName)
        {
            Name = name;
            IpAddress = ipAddress;
            Id = id;
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
        /// Gets or sets the name of the EC2 instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the EC2 instance.
        /// </summary>
        [Display(Name = "Ip Address")]
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the ID of the EC2 instance.
        /// </summary>
        [Display(Name = "Instance Id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the status of the EC2 instance.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the EC2 instance can be rebooted.
        /// </summary>
        public bool CanReboot { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the EC2 instance can be stopped.
        /// </summary>
        public bool CanStop { get; set; } = false;
    }
}
