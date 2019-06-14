using CommandLine;

namespace Ec2Manager.Models.CommandLine
{
    public class EncryptCommand
    {
        [Option(HelpText = "The Name of the Account",Required = true)]
        public string AccountName { get; set; }
        [Option(HelpText = "The unencrypted AccessKey", Required = true)]
        public string AccessKey { get; set; }
        [Option(HelpText = "The unencryptede SecretKey", Required = true)]
        public string SecretKey { get; set; }
        [Option(HelpText = "The Directory to place the KeyFile", Required = true)]
        public string OutputDirectory { get; set; }
    }
}
