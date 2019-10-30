namespace Ec2Manager.Models
{
    public class AwsAccount
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string AccessKeyHash { get; set; }
        public string SecretKeyHash { get; set; }
        public string TagToSearch { get; set; }
        public string TagSearchString { get; set; }
        public string NameTag { get; set; }
    }
}
