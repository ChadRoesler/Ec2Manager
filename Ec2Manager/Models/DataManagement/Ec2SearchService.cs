using Ec2Manager.Models.ConfigManagement;
using PagedList.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ec2Manager.Models.DataManagement
{
    public class Ec2SearchService
    {
        private readonly IEnumerable<Ec2Instance> ec2SearchData;
        private readonly IEnumerable<ClaimValueAccount> claimValueData;

        public Ec2SearchService(IEnumerable<Ec2Instance> ec2Instances, IEnumerable<ClaimValueAccount> claimValueAccounts)
        {
            ec2SearchData = ec2Instances;
            claimValueData = claimValueAccounts;
        }

        public Ec2SearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder)
        {
            var masterAccountList = claimValueData.SelectMany(x => x.Accounts).ToHashSet();
            searchType = string.IsNullOrWhiteSpace(searchType) ? "name" : searchType.ToLower();
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name" : sortOrder.ToLower();
            query = string.IsNullOrWhiteSpace(query) ? "" : query;

            var searchHits = ec2SearchData.Where(x => masterAccountList.Contains(x.Account));

            if (!string.IsNullOrWhiteSpace(query))
            {
                searchHits = searchType switch
                {
                    "ipaddress" => searchHits.Where(x => Regex.IsMatch(x.IpAddress, query, RegexOptions.IgnoreCase)),
                    "status" => searchHits.Where(x => Regex.IsMatch(x.Status, query, RegexOptions.IgnoreCase)),
                    "id" => searchHits.Where(x => Regex.IsMatch(x.Id, query, RegexOptions.IgnoreCase)),
                    "account" => searchHits.Where(x => Regex.IsMatch(x.Account, query, RegexOptions.IgnoreCase)),
                    _ => searchHits.Where(x => Regex.IsMatch(x.Name, query, RegexOptions.IgnoreCase)),
                };
            }

            searchHits = sortOrder switch
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

            var searchHitsList = searchHits.ToList();
            foreach (var ec2 in searchHitsList)
            {
                ec2.CanReboot = claimValueData.Any(y => y.Accounts.Contains(ec2.Account) && y.EnableReboot);
                ec2.CanStop = claimValueData.Any(y => y.Accounts.Contains(ec2.Account) && y.EnableStop);
            }

            var searchResult = new Ec2SearchResult
            {
                SearchHits = new StaticPagedList<Ec2Instance>(searchHitsList.Skip((page - 1) * pageSize).Take(pageSize), page, pageSize, searchHitsList.Count),
                SearchQuery = query,
                Page = page,
                SortOrder = sortOrder,
                SearchType = searchType
            };

            return searchResult;
        }
    }
}
