﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PagedList.Core;
using Ec2Manager.Interfaces;
using Ec2Manager.Models.ConfigManagement;

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
                switch (searchType.ToLower())
                {
                    case "ipaddress":
                        searchHits = searchHits.Where(x => Regex.Match(x.IpAddress, query, RegexOptions.IgnoreCase).Success);
                        break;
                    case "status":
                        searchHits = searchHits.Where(x => Regex.Match(x.Status, query, RegexOptions.IgnoreCase).Success);
                        break;
                    case "id":
                        searchHits = searchHits.Where(x => Regex.Match(x.Id, query, RegexOptions.IgnoreCase).Success);
                        break;
                    case "account":
                        searchHits = searchHits.Where(x => Regex.Match(x.Account, query, RegexOptions.IgnoreCase).Success);
                        break;
                    case "name":
                    default:
                        searchHits = searchHits.Where(x => Regex.Match(x.Name, query, RegexOptions.IgnoreCase).Success);
                        break;
                }
                
            }
            
            switch (sortOrder.ToLower())
            {
                case "status":
                    searchHits = searchHits.OrderBy(x => x.Status);
                    break;
                case "status_desc":
                    searchHits = searchHits.OrderByDescending(x => x.Status);
                    break;
                case "ipaddress":
                    searchHits = searchHits.OrderBy(x => x.IpAddress);
                    break;
                case "ipaddress_desc":
                    searchHits = searchHits.OrderByDescending(x => x.IpAddress);
                    break;
                case "id":
                    searchHits = searchHits.OrderBy(x => x.Id);
                    break;
                case "id_desc":
                    searchHits = searchHits.OrderByDescending(x => x.Id);
                    break;
                case "name_desc":
                    searchHits = searchHits.OrderByDescending(x => x.Name);
                    break;
                case "name":
                default:
                    searchHits = searchHits.OrderBy(x => x.Name);
                    break;
            }
            searchHits.ToList().ForEach(x => x.CanReboot = claimValueData.SingleOrDefault(x => x.EnableReboot) != null);

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
