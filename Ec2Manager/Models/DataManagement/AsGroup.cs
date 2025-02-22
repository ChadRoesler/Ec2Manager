using System.ComponentModel.DataAnnotations;

namespace Ec2Manager.Models.DataManagement
{
    /// <summary>
    /// Represents an Auto Scaling Group (ASG) in AWS.
    /// </summary>
    public class AsGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsGroup"/> class.
        /// </summary>
        /// <param name="name">The name of the ASG.</param>
        /// <param name="instanceCount">The number of instances in the ASG.</param>
        /// <param name="instanceRefresh">Indicates whether the ASG is refreshing instances.</param>
        /// <param name="accountName">The name of the AWS account.</param>
        /// <param name="desiredCapacityValue">The desired capacity value of the ASG.</param>
        /// <param name="maxCapacityValue">The maximum capacity value of the ASG.</param>
        /// <param name="minCapacityValue">The minimum capacity value of the ASG.</param>
        public AsGroup(string name, int instanceCount, int currentDesiredCapacity, bool instanceRefresh, string accountName, int desiredCapacityValue = 0, int maxCapacityValue = 0, int minCapacityValue = 0)
        {
            Name = name;
            InstanceCount = instanceCount;
            Status = instanceRefresh ? "refreshing" : instanceCount > 0 ? "running" : currentDesiredCapacity > 0 ? "starting" : "stopped";
            Account = accountName;
            CurrentDesiredCapacity = currentDesiredCapacity;
            DesiredCapacityValue = desiredCapacityValue;
            MaxCapacityValue = maxCapacityValue;
            MinCapacityValue = minCapacityValue;
        }

        /// <summary>
        /// Gets the status image based on the current status.
        /// </summary>
        public string StatusImage => Status.ToLowerInvariant() switch
        {
            "stopped" => "images/stopped.png",
            "running" => "images/started.png",
            "starting" => "images/changing.png",
            "refreshing" => "images/changing.png",
            _ => "images/unknown.png"
        };

        /// <summary>
        /// Gets or sets the AWS account name.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the name of the ASG.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of instances in the ASG.
        /// </summary>
        [Display(Name = "Instance Count")]
        public int InstanceCount { get; set; }

        public int CurrentDesiredCapacity { get; set; }

        [Display(Name = "Current of Total")]
        public string CurrentOfTotalCapacity { get { return $"{InstanceCount} of {CurrentDesiredCapacity}"; } }

        /// <summary>
        /// Gets or sets the status of the ASG.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ASG can be refreshed.
        /// </summary>
        public bool CanRefresh { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the ASG can be stopped.
        /// </summary>
        public bool CanStop { get; set; } = false;

        /// <summary>
        /// Gets or sets the desired capacity value of the ASG.
        /// </summary>
        public int DesiredCapacityValue { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum capacity value of the ASG.
        /// </summary>
        public int MaxCapacityValue { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum capacity value of the ASG.
        /// </summary>
        public int MinCapacityValue { get; set; } = 0;
    }
}
