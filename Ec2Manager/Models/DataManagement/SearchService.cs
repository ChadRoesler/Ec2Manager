using Ec2Manager.Interfaces;
using Ec2Manager.Models.ConfigManagement;
using PagedList.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ec2Manager.Models.DataManagement
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<Ec2Instance> searchData = new List<Ec2Instance>();
        private readonly IEnumerable<ClaimValueAccount> claimValueData = new List<ClaimValueAccount>();

        public SearchService(IEnumerable<Ec2Instance> ec2Instances, IEnumerable<ClaimValueAccount> claimValueAccounts)
        {
            searchData = ec2Instances;
            claimValueData = claimValueAccounts;
        }

        public SearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder)
        {
            var masterAccountList = new List<string>();
            claimValueData.ToList().ForEach(x => masterAccountList.AddRange(x.Accounts));
            searchType = string.IsNullOrWhiteSpace(searchType) ? "name" : searchType;
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name" : sortOrder;
            query = string.IsNullOrWhiteSpace(query) ? "" : query;
            var searchHits = searchData.Where(x => masterAccountList.Contains(x.Account));
            if (!string.IsNullOrWhiteSpace(query))
            {
                searchHits = (searchType.ToLower()) switch
                {
                    "ipaddress" => searchHits.Where(x => Regex.Match(x.IpAddress, query, RegexOptions.IgnoreCase).Success),
                    "status" => searchHits.Where(x => Regex.Match(x.Status, query, RegexOptions.IgnoreCase).Success),
                    "id" => searchHits.Where(x => Regex.Match(x.Id, query, RegexOptions.IgnoreCase).Success),
                    "account" => searchHits.Where(x => Regex.Match(x.Account, query, RegexOptions.IgnoreCase).Success),
                    _ => searchHits.Where(x => Regex.Match(x.Name, query, RegexOptions.IgnoreCase).Success),
                };
            }

            searchHits = (sortOrder.ToLower()) switch
            {
                "status" => searchHits.OrderBy(x => x.Status),
                "status_desc" => searchHits.OrderByDescending(x => x.Status),
                "ipaddress" => searchHits.OrderBy(x => x.IpAddress),
                "ipaddress_desc" => searchHits.OrderByDescending(x => x.IpAddress),
                "id" => searchHits.OrderBy(x => x.Id),
                "id_desc" => searchHits.OrderByDescending(x => x.Id),
                "account" => searchHits.OrderBy(x => x.Account),
                "account_desc" => searchHits.OrderByDescending(x => x.Account),
                "name_desc" => searchHits.OrderByDescending(x => x.Name),
                _ => searchHits.OrderBy(x => x.Name),
            };
            searchHits.ToList().ForEach(x => x.CanReboot = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableReboot) != null);
            searchHits.ToList().ForEach(x => x.CanStop = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableStop) != null);

            var searchResult = new SearchResult()
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
