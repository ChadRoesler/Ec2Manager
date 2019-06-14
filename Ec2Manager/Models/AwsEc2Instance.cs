using System;
using System.ComponentModel.DataAnnotations;

namespace Ec2Manager.Models
{
    public class AwsEc2Instance
    {
        internal AwsEc2Instance()
        {

        }
        public AwsEc2Instance(string Name, string IpAddress, string Id, string Status, string AccountName)
        {
            this.Name = Name;
            this.IpAddress = IpAddress;
            this.Id = Id;
            this.Status = Status;
            this.AccountName = AccountName;
        }
        public string StatusImage
        {
            get
            {
                if (Status.Equals("Stopped", StringComparison.InvariantCultureIgnoreCase))
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
                else if (Status.Equals("stopping", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/stopped.png";
                }
                else if(Status.Equals("terminating", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/unknown.png";
                }
                else if(Status.Equals("terminated", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "images/unknown.png";
                }
                else
                {
                    return "images/unknown.png";
                }
            }
        }
        [Display(Name = "Account")]
        public string AccountName { get; set; }
        public string Name { get; set; }
        [Display(Name = "Ip Address")]
        public string IpAddress { get; set; }
        [Display(Name = "Instance Id")]
        public string Id { get; set; }
        public string Status { get; set; }
    }
}
