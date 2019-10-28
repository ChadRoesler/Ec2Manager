namespace Ec2Manager.Models
{
    public class Ec2Manager
    {
        public int ResultsPerPage { get; set; }
        public AwsAccount[] Accounts { get; set; }
    }
}
