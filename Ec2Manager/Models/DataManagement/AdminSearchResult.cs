using System.Collections.Generic;

namespace Ec2Manager.Models.DataManagement
{
    public class AdminSearchResult
    {
        public IEnumerable<Ec2Instance> SearchHits { get; set; }

        public string SearchQuery { get; set; }

        public string SortOrder { get; set; }

        public string SearchType { get; set; }
    }
}
