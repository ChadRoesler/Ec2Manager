using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ec2Manager.Models.ConfigManagement
{
    public class ClaimValueAccount
    {
        public string Value { get; set; }
        public IEnumerable<string> Accounts { get; set; }

        public bool EnableReboot { get; set; }
    }
}
