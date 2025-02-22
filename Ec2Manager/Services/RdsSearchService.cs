using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;
using PagedList.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ec2Manager.Services
{
    public class RdsSearchService
    {
        private readonly IEnumerable<RdsObject> rdsSearchData;
        private readonly IEnumerable<ClaimValueAccount> claimValueData;

        public RdsSearchService(IEnumerable<RdsObject> rdsInstances, IEnumerable<ClaimValueAccount> claimValueAccounts)
        {
            rdsSearchData = rdsInstances;
            claimValueData = claimValueAccounts;
        }

        public RdsSearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder)
        {
            var masterAccountList = claimValueData.SelectMany(x => x.Accounts).ToHashSet();
            searchType = string.IsNullOrWhiteSpace(searchType) ? "name" : searchType.ToLower();
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name" : sortOrder.ToLower();
            query = string.IsNullOrWhiteSpace(query) ? "" : query;

            var searchHits = rdsSearchData.Where(x => masterAccountList.Contains(x.Account));

            if (!string.IsNullOrWhiteSpace(query))
            {
                searchHits = searchType switch
                {
                    "endpoint" => searchHits.Where(x => Regex.IsMatch(x.Endpoint, query, RegexOptions.IgnoreCase)),
                    "status" => searchHits.Where(x => Regex.IsMatch(x.Status, query, RegexOptions.IgnoreCase)),
                    "account" => searchHits.Where(x => Regex.IsMatch(x.Account, query, RegexOptions.IgnoreCase)),
                    _ => searchHits.Where(x => Regex.IsMatch(x.DbIdentifier, query, RegexOptions.IgnoreCase)),
                };
            }

            searchHits = sortOrder switch
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

            var searchHitsList = searchHits.ToList();
            foreach (var rds in searchHitsList)
            {
                rds.CanReboot = claimValueData.Any(y => y.Accounts.Contains(rds.Account) && y.EnableReboot);
                rds.CanStop = claimValueData.Any(y => y.Accounts.Contains(rds.Account) && y.EnableStop);
            }

            var searchResult = new RdsSearchResult
            {
                SearchHits = new StaticPagedList<RdsObject>(searchHitsList.Skip((page - 1) * pageSize).Take(pageSize), page, pageSize, searchHitsList.Count),
                SearchQuery = query,
                Page = page,
                SortOrder = sortOrder,
                SearchType = searchType
            };

            return searchResult;
        }
    }
}
