namespace Ec2Manager.Interfaces
{
    interface IAwsAccountInfo
    {
        string AccountName { get; set; }
        string RoleArn { get; set; }
        string NameTag { get; set; }
        string Region { get; set; }
        string TagToSearch { get; set; }
        string SearchString { get; set; }
    }
}
