using System.Collections.Generic;
using System.Linq;
using Ec2Manager.Interfaces;
using PagedList.Core;

namespace Ec2Manager.Models.DataManagement
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<Ec2Instance> searchData = new List<Ec2Instance>();

        public SearchService(IEnumerable<Ec2Instance> ec2Instances)
        {
            searchData = ec2Instances;
        }

        public SearchResult GetSearchResult(string query, int page, int pageSize)
        {
            var searchHits = searchData;
            if (!string.IsNullOrWhiteSpace(query))
            {
                searchHits = searchHits.Where(x => x.Name.Contains(query, System.StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                query = "";
            }
            

            var searchResult = new SearchResult()
            {
                SearchHits = new StaticPagedList<Ec2Instance>(searchHits.Skip((page - 1) * pageSize).Take(pageSize), page, pageSize, searchHits.Count()),
                SearchQuery = query,
                Page = page
            };

            return searchResult;
        }
    }
}
