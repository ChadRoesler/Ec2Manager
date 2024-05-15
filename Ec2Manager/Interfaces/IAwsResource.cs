namespace Ec2Manager.Interfaces
{
    public interface IAwsResource
    {
        string Account { get; set; }
        string Status { get; set; }
        string StatusImage { get; }
        bool CanReboot { get; set; }
        bool CanStop { get; set; }
    }
}
