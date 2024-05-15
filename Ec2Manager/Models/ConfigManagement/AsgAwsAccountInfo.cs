using Ec2Manager.Interfaces;

namespace Ec2Manager.Models.ConfigManagement
{
    public class AsgAwsAccountInfo : IAwsAccountInfo
    {
        public string AccountName { get; set; }
        public string RoleArn { get; set; }
        public string Region { get; set; }
        public string TagToSearch { get; set; }
        public string SearchString { get; set; }
    }
}
