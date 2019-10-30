namespace Ec2Manager.Models
{
    public class Ec2Manager
    {
        public bool EnableReboot { get; set; }
        public int ResultsPerPage { get; set; }
        public AwsAccount[] Accounts { get; set; }
    }
}
