using Ec2Manager.Models.ConfigManagement;
using PagedList.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ec2Manager.Models.DataManagement
{
    public class RdsSearchService
    {
        private readonly IEnumerable<RdsInstance> rdsSearchData = new List<RdsInstance>();
        private readonly IEnumerable<ClaimValueAccount> claimValueData = new List<ClaimValueAccount>();

        public RdsSearchService(IEnumerable<RdsInstance> rdsInstances, IEnumerable<ClaimValueAccount> claimValueAccounts)
        {
            rdsSearchData = rdsInstances;
            claimValueData = claimValueAccounts;
        }

        public RdsSearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder)
        {
            List<string> masterAccountList = new();
            claimValueData.ToList().ForEach(x => masterAccountList.AddRange(x.Accounts));
            searchType = string.IsNullOrWhiteSpace(searchType) ? "name" : searchType;
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name" : sortOrder;
            query = string.IsNullOrWhiteSpace(query) ? "" : query;
            IEnumerable<RdsInstance> searchHits = rdsSearchData.Where(x => masterAccountList.Contains(x.Account));
            if (!string.IsNullOrWhiteSpace(query))
            {
                searchHits = (searchType.ToLower()) switch
                {
                    "endpoint" => searchHits.Where(x => Regex.Match(x.Endpoint, query, RegexOptions.IgnoreCase).Success),
                    "status" => searchHits.Where(x => Regex.Match(x.Status, query, RegexOptions.IgnoreCase).Success),
                    "account" => searchHits.Where(x => Regex.Match(x.Account, query, RegexOptions.IgnoreCase).Success),
                    _ => searchHits.Where(x => Regex.Match(x.DbIdentifier, query, RegexOptions.IgnoreCase).Success),
                };
            }

            searchHits = (sortOrder.ToLower()) switch
            {
                "status" => searchHits.OrderBy(x => x.Status),
                "status_desc" => searchHits.OrderByDescending(x => x.Status),
                "endpoint" => searchHits.OrderBy(x => x.Endpoint),
                "endpoint_desc" => searchHits.OrderByDescending(x => x.Endpoint),
                "account" => searchHits.OrderBy(x => x.Account),
                "account_desc" => searchHits.OrderByDescending(x => x.Account),
                "name_desc" => searchHits.OrderByDescending(x => x.DbIdentifier),
                _ => searchHits.OrderBy(x => x.DbIdentifier),
            };
            searchHits.ToList().ForEach(x => x.CanReboot = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableReboot) != null);
            searchHits.ToList().ForEach(x => x.CanStop = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableStop) != null);

            RdsSearchResult searchResult = new()
            {
                SearchHits = new StaticPagedList<RdsInstance>(searchHits.Skip((page - 1) * pageSize).Take(pageSize), page, pageSize, searchHits.Count()),
                SearchQuery = query,
                Page = page,
                SortOrder = sortOrder,
                SearchType = searchType
            };

            return searchResult;
        }
    }
}
