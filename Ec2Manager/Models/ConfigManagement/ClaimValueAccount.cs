using System.Collections.Generic;

namespace Ec2Manager.Models.ConfigManagement
{
    /// <summary>
    /// Represents a claim value account configuration.
    /// </summary>
    public class ClaimValueAccount
    {
        /// <summary>
        /// Gets the claim value.
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// Gets the list of associated accounts.
        /// </summary>
        public IEnumerable<string> Accounts { get; init; }

        /// <summary>
        /// Gets a value indicating whether reboot is enabled.
        /// </summary>
        public bool EnableReboot { get; init; } = false;

        /// <summary>
        /// Gets a value indicating whether stop is enabled.
        /// </summary>
        public bool EnableStop { get; init; } = false;
    }
}
