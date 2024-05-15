using Ec2Manager.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ec2Manager.Models.DataManagement
{
    public class AsGroup : IAwsResource
    {
        public AsGroup(string Name, int InstanceCount, bool InstanceRefresh, string AccountName)
        {
            this.Name = Name;
            this.InstanceCount = InstanceCount;
            Status = InstanceRefresh ? "refreshing" : InstanceCount switch
            {
                > 0 => "running",
                _ => "stopped"
            };
            Account = AccountName;
        }
        public string StatusImage
        {
            get
            {
                if (Status.Equals("stopped", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/stopped.png";
                }
                if (Status.Equals("running", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/started.png";
                }
                if (Status.Equals("refreshing", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/changing.png";
                }
                return "images/unknown.png";
            }
        }
        public string Account { get; set; }
        public string Name { get; set; }
        public int InstanceCount { get; set; }
        public string Status { get; set; }
        public bool CanReboot { get; set; } = false;
        public bool CanStop { get; set; } = false;
    }
}
