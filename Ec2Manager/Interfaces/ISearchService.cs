using System.Collections.Generic;
using Ec2Manager.Models.DataManagement;
using Ec2Manager.Models.ConfigManagement;

namespace Ec2Manager.Interfaces
{
    public interface ISearchService
    {
        //SearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder);
        SearchResult GetSearchResult(SearchData searchData);
        AdminSearchResult GetAdminSearchResult(string searchType, string query, string sortOrder);
    }
}
