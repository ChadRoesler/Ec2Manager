namespace Ec2Manager.Interfaces
{
    interface IAwsAccountInfo
    {
        string AccountName { get; set; }
        string SecretName { get; set; }
        string NameTag { get; set; }
        string Region { get; set; }
        string TagToSearch { get; set; }
        string SearchString { get; set; }
        string AccessKey { get; set; }
        string SecretKey { get; set; }
    }
}
