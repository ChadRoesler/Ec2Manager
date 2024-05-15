using PagedList.Core;

namespace Ec2Manager.Models.DataManagement
{
    public class AsGroupSearchResult
    {
        public IPagedList<AsGroup> SearchHits { get; set; }

        public string SearchQuery { get; set; }

        public int Page { get; set; }

        public string SortOrder { get; set; }

        public string SearchType { get; set; }
    }
}
