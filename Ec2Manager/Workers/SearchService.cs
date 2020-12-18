using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PagedList.Core;
using Ec2Manager.Interfaces;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;

namespace Ec2Manager.Workers
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<Ec2Instance> instanceList = new List<Ec2Instance>();
        private readonly IEnumerable<ClaimValueAccount> claimValueData = new List<ClaimValueAccount>();

        public SearchService(IEnumerable<Ec2Instance> ec2Instances, IEnumerable<ClaimValueAccount> claimValueAccounts)
        {
            instanceList = ec2Instances;
            claimValueData = claimValueAccounts;
        }

        public SearchResult GetSearchResult(SearchData searchData)
        {
            var masterAccountList = new List<string>();
            claimValueData.ToList().ForEach(x => masterAccountList.AddRange(x.Accounts));
            
            var searchHits = instanceList.Where(x => masterAccountList.Contains(x.Account));
            if (!string.IsNullOrWhiteSpace(searchData.SearchType))
            {

                searchHits = (searchData.SearchType.ToLower()) switch
                {
                    "ipaddress" => searchHits.Where(x => Regex.Match(x.IpAddress, searchData.SearchQuery, RegexOptions.IgnoreCase).Success),
                    "status" => searchHits.Where(x => Regex.Match(x.Status, searchData.SearchQuery, RegexOptions.IgnoreCase).Success),
                    "id" => searchHits.Where(x => Regex.Match(x.Id, searchData.SearchQuery, RegexOptions.IgnoreCase).Success),
                    "account" => searchHits.Where(x => Regex.Match(x.Account, searchData.SearchQuery, RegexOptions.IgnoreCase).Success),
                    _ => searchHits.Where(x => Regex.Match(x.Name ?? "", searchData.SearchQuery, RegexOptions.IgnoreCase).Success),
                };
            }

            searchHits = (searchData.SortOrder.ToLower()) switch
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
            searchHits.ToList().ForEach(x => x.CanStart = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableStart) != null);
            searchHits.ToList().ForEach(x => x.CanReboot = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableReboot) != null);
            searchHits.ToList().ForEach(x => x.CanStop = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableStop) != null);
            var searchResult = new SearchResult()
            {
                SearchHits = new StaticPagedList<Ec2Instance>(searchHits.Skip((searchData.Page - 1) * searchData.PageSize).Take(searchData.PageSize), searchData.Page, searchData.PageSize, searchHits.Count()),
                SearchData = searchData
            };

            return searchResult;
        }

        public AdminSearchResult GetAdminSearchResult(string searchType, string query, string sortOrder)
        {
            var masterAccountList = new List<string>();
            claimValueData.ToList().ForEach(x => masterAccountList.AddRange(x.Accounts));
            searchType = string.IsNullOrWhiteSpace(searchType) ? "name" : searchType;
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name" : sortOrder;
            query = string.IsNullOrWhiteSpace(query) ? "" : query;
            var searchHits = instanceList.Where(x => masterAccountList.Contains(x.Account));
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
            searchHits.ToList().ForEach(x => x.CanStart = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableStart) != null);
            searchHits.ToList().ForEach(x => x.CanReboot = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableReboot) != null);
            searchHits.ToList().ForEach(x => x.CanStop = claimValueData.SingleOrDefault(y => y.Accounts.Contains(x.Account) && y.EnableStop) != null);

            var searchResult = new AdminSearchResult()
            {
                SearchHits = searchHits,
                SearchQuery = query,
                SortOrder = sortOrder,
                SearchType = searchType
            };

            return searchResult;
        }
    }
}
