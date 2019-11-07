using Ec2Manager.Models.DataManagement;

namespace Ec2Manager.Interfaces
{
    public interface ISearchService
    {
        SearchResult GetSearchResult(string query, int page, int pageSize);
    }
}
