namespace Ec2Manager.Interfaces
{
    interface IAzureAccountInfo
    {
        string AccountName { get; set; }
        string RoleRi { get; set; }
        string NameTag { get; set; }
        string Region { get; set; }
        string TagToSearch { get; set; }
        string SearchString { get; set; }
    }
}
