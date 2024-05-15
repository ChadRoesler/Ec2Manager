using Ec2Manager.Models.ConfigManagement;
using PagedList.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ec2Manager.Models.DataManagement
{
    public class AsGroupSearchService
    {
        private readonly IEnumerable<AsGroup> asgSearchData = new List<AsGroup>();
        private readonly IEnumerable<ClaimValueAccount> claimValueData = new List<ClaimValueAccount>();

        public AsGroupSearchService(IEnumerable<AsGroup> asgInstances, IEnumerable<ClaimValueAccount> claimValueAccounts)
        {
            asgSearchData = asgInstances;
            claimValueData = claimValueAccounts;
        }

        public Ec2SearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder)
        {
            List<string> masterAccountList = new();
            claimValueData.ToList().ForEach(x => masterAccountList.AddRange(x.Accounts));
            searchType = string.IsNullOrWhiteSpace(searchType) ? "name" : searchType;
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name" : sortOrder;
            query = string.IsNullOrWhiteSpace(query) ? "" : query;
            IEnumerable<AsGroup> searchHits = asgSearchData.Where(x => masterAccountList.Contains(x.Account));
            if (!string.IsNullOrWhiteSpace(query))
            {
                searchHits = (searchType.ToLower()) switch
                {
                    "status" => searchHits.Where(x => Regex.Match(x.Status, query, RegexOptions.IgnoreCase).Success),
                    "account" => searchHits.Where(x => Regex.Match(x.Account, query, RegexOptions.IgnoreCase).Success),
                    _ => searchHits.Where(x => Regex.Match(x.Name, query, RegexOptions.IgnoreCase).Success),
                };
            }

            searchHits = (sortOrder.ToLower()) switch
            {
                "status" => searchHits.OrderBy(x => x.Status),
                "status_desc" => searchHits.OrderByDescending(x => x.Status),
                "account" => searchHits.OrderBy(x => x.Account),
                "account_desc" => searchHits.OrderByDescending(x => x.Account),
                "name_desc" => searchHits.OrderByDescending(x => x.Name),
                _ => searchHits.OrderBy(x => x.Name),
            };
            searchHits.ToList().ForEach(x => x.CanReboot = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableReboot) != null);
            searchHits.ToList().ForEach(x => x.CanStop = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableStop) != null);

            Ec2SearchResult searchResult = new()
            {
                SearchHits = new StaticPagedList<Ec2Instance>(searchHits.Skip((page - 1) * pageSize).Take(pageSize), page, pageSize, searchHits.Count()),
                SearchQuery = query,
                Page = page,
                SortOrder = sortOrder,
                SearchType = searchType
            };

            return searchResult;
        }
    }
}
