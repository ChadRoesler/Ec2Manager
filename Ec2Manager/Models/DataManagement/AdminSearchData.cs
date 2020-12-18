using System.ComponentModel;

namespace Ec2Manager.Models.DataManagement
{
    public class AdminSearchData
    {
        [DefaultValue("*")]
        public string SearchQuery { get; set; }

        [DefaultValue("Name")]
        public string SortOrder { get; set; }

        [DefaultValue("Name")]
        public string SearchType { get; set; }
    }
}
