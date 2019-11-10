using Ec2Manager.Interfaces;

namespace Ec2Manager.Models.DataManagement
{
    public class AwsAccountInfoModel : IAwsAccountInfo
    {
        public string AccountName { get; set; }
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string TagToSearch { get; set; }
        public string SearchString { get; set; }
        public string NameTag { get; set; }
    }
}
