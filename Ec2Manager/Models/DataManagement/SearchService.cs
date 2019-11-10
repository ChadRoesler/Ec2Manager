using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PagedList.Core;
using Ec2Manager.Interfaces;
using System;

namespace Ec2Manager.Models.DataManagement
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<Ec2Instance> searchData = new List<Ec2Instance>();

        public SearchService(IEnumerable<Ec2Instance> ec2Instances)
        {
            searchData = ec2Instances;
        }

        public SearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder)
        {
            if(string.IsNullOrWhiteSpace(searchType))
            {
                searchType = "name";
            }
            var searchHits = searchData;
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
            else
            {
                query = "";
            }
            if(string.IsNullOrWhiteSpace(sortOrder))
            {
                sortOrder = "name";
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
