using PagedList.Core;

namespace Ec2Manager.Models.DataManagement
{
    /// <summary>
    /// Represents the search result for Auto Scaling Groups (ASG).
    /// </summary>
    public class AsGroupSearchResult
    {
        /// <summary>
        /// Gets or sets the search hits as a paged list of ASG.
        /// </summary>
        public IPagedList<AsGroup> SearchHits { get; set; }

        /// <summary>
        /// Gets or sets the search query.
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        public string SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the search type.
        /// </summary>
        public string SearchType { get; set; }
    }
}
