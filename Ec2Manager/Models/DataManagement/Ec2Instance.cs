using System;
using System.ComponentModel.DataAnnotations;

namespace Ec2Manager.Models.DataManagement
{
    public class Ec2Instance
    {
        public Ec2Instance(string Name, string IpAddress, string Id, string Status, string AccountName)
        {
            this.Name = Name;
            this.IpAddress = IpAddress;
            this.Id = Id;
            this.Status = Status;
            this.Account = AccountName;
        }
        public string StatusImage
        {
            get
            {
                if (Status.Equals("stopped", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/stopped.png";
                }
                else if (Status.Equals("stopping", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/stopped.png";
                }
                else if (Status.Equals("running", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/started.png";
                }
                else if (Status.Equals("starting", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/started.png";
                }
                else if (Status.Equals("shutting-down", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/changing.png";
                }
                else if (Status.Equals("terminated", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/unknown.png";
                }
                else if (Status.Equals("initializing", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/changing.png";
                }
                else if (Status.Equals("impaired", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/warning.png";
                }
                else
                {
                    return "images/unknown.png";
                }
            }
        }
        public string Account { get; set; }
        public string Name { get; set; }
        [Display(Name = "Ip Address")]
        public string IpAddress { get; set; }
        [Display(Name = "Instance Id")]
        public string Id { get; set; }
        public string Status { get; set; }
        public bool CanReboot { get; set; } = false;
        public bool CanStop { get; set; } = false;
    }
}
