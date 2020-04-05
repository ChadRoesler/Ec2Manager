using System.Collections.Generic;

namespace Ec2Manager.Models.ConfigManagement
{
    public class ClaimValueAccount
    {
        public string Value { get; set; }
        public IEnumerable<string> Accounts { get; set; }

        public bool EnableReboot { get; set; }

        public bool EnableStop { get; set; }
    }
}
