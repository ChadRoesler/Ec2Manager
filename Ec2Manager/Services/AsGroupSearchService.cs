﻿using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;
using PagedList.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ec2Manager.Services
{
    internal class AsGroupSearchService
    {
        private readonly IEnumerable<AsGroup> asgSearchData;
        private readonly IEnumerable<ClaimValueAccount> claimValueData;

        internal AsGroupSearchService(IEnumerable<AsGroup> asgInstances, IEnumerable<ClaimValueAccount> claimValueAccounts)
        {
            asgSearchData = asgInstances;
            claimValueData = claimValueAccounts;
        }

        internal AsGroupSearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder)
        {
            var masterAccountList = claimValueData.SelectMany(x => x.Accounts).ToHashSet();
            searchType = string.IsNullOrWhiteSpace(searchType) ? "name" : searchType.ToLower();
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name" : sortOrder.ToLower();
            query = string.IsNullOrWhiteSpace(query) ? "" : query;

            var searchHits = asgSearchData.Where(x => masterAccountList.Contains(x.Account));

            if (!string.IsNullOrWhiteSpace(query))
            {
                searchHits = searchType switch
                {
                    "status" => searchHits.Where(x => Regex.IsMatch(x.Status, query, RegexOptions.IgnoreCase)),
                    "account" => searchHits.Where(x => Regex.IsMatch(x.Account, query, RegexOptions.IgnoreCase)),
                    _ => searchHits.Where(x => Regex.IsMatch(x.Name, query, RegexOptions.IgnoreCase)),
                };
            }

            searchHits = sortOrder switch
            {
                "status" => searchHits.OrderBy(x => x.Status),
                "status_desc" => searchHits.OrderByDescending(x => x.Status),
                "account" => searchHits.OrderBy(x => x.Account),
                "account_desc" => searchHits.OrderByDescending(x => x.Account),
                "name_desc" => searchHits.OrderByDescending(x => x.Name),
                "instancecount" => searchHits.OrderBy(x => x.InstanceCount),
                "instancecount_desc" => searchHits.OrderByDescending(x => x.InstanceCount),
                _ => searchHits.OrderBy(x => x.Name),
            };

            var searchHitsList = searchHits.ToList();
            foreach (var asg in searchHitsList)
            {
                asg.CanRefresh = claimValueData.Any(y => y.Accounts.Contains(asg.Account) && y.EnableReboot);
                asg.CanStop = claimValueData.Any(y => y.Accounts.Contains(asg.Account) && y.EnableStop);
            }

            var searchResult = new AsGroupSearchResult
            {
                SearchHits = new StaticPagedList<AsGroup>(searchHitsList.Skip((page - 1) * pageSize).Take(pageSize), page, pageSize, searchHitsList.Count),
                SearchQuery = query,
                Page = page,
                SortOrder = sortOrder,
                SearchType = searchType
            };

            return searchResult;
        }
    }
}
