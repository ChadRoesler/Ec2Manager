using CommandLine;
using Ec2Manager.Interfaces;

namespace Ec2Manager.Models.CommandLine
{
    public class EncryptCommand : IAwsAccountInfo
    {
        [Option('n', "AccountName", HelpText = "The Name of the Account", Required = true)]
        public string AccountName { get; set; }
        [Option('a', "AccessKey", HelpText = "The unencrypted AccessKey", Required = true)]
        public string AccessKey { get; set; }
        [Option('s', "SecretKey", HelpText = "The unencryptede SecretKey", Required = true)]
        public string SecretKey { get; set; }
        [Option('r', "Region", HelpText = "The Region of the Ec2Instances to gather from", Required = false, Default = "us-east-2")]
        public string Region { get; set; }
        [Option('d', "NameTag", HelpText = "The Tag to be displayed in the Name Column", Required = false, Default = "Name")]
        public string NameTag { get; set; }
        [Option('t', "TagToSearch", HelpText = "The Tag to search on for listing Instances", Required = false, Default = "Name")]
        public string TagToSearch { get; set; }
        [Option('x', "SearchString", HelpText = "The RegEx String used for searching in the TagToSearch", Required = false, Default = ".*")]
        public string SearchString { get; set; }
        [Option('o', "OutputDirectory", HelpText = "The Directory to place the KeyFile", Required = true)]
        public string OutputDirectory { get; set; }
        [Option('z', "AppSettingOnly", HelpText = "Only Output the appsettings.json value", Required = false, Default = false)]
        public bool AppSettingOnly { get; set; }
    }
}
