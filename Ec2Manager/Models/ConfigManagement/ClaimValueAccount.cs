using System.Collections.Generic;
using System.ComponentModel;

namespace Ec2Manager.Models.ConfigManagement
{
    public class ClaimValueAccount
    {
        public string Value { get; set; }
        public IEnumerable<string> Accounts { get; set; }

        [DefaultValue(false)]
        public bool EnableStart { get; set; }

        [DefaultValue(false)]
        public bool EnableReboot { get; set; }

        [DefaultValue(false)]
        public bool EnableStop { get; set; }

        [DefaultValue(false)]
        public bool AdminAccess { get; set; }

    }
}
