using System.ComponentModel;

namespace Ec2Manager.Models.DataManagement
{
    public class SearchData
    {
        public string SearchQuery { get; set; } = "";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 5;

        public string SortOrder { get; set; } = "name";

        public string SearchType { get; set; } = "name";
    }
}
