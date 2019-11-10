using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList.Core;
using System.Collections.Generic;

namespace Ec2Manager.Models.DataManagement
{
    public class SearchResult
    {
        public IPagedList<Ec2Instance> SearchHits { get; set; }

        public string SearchQuery { get; set; }

        public int Page { get; set; }
        public string PageSize { get; set; }

        public string SortOrder { get; set; }
        public string SearchType { get; set; }

        public List<SelectListItem> PageSizeList
        {
            get
            {
                return new List<SelectListItem>()
                 {
                     new SelectListItem("5","5"),
                     new SelectListItem("10","10"),
                     new SelectListItem("15", "15"),
                     new SelectListItem("All", "All")
                 };
            }
        }
    }
}
