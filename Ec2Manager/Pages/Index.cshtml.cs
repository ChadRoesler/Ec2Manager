﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Ec2Manager.Models;
using Ec2Manager.Workers;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ec2Manager.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IOptions<AppConfig> config;

        public IndexModel(IOptions<AppConfig> config)
        { 
            this.config = config;
        }

        public string NameSort { get; set; }
        public string StatusSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentSearchType { get; set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            StatusSort = sortOrder == "status" ? "status_desc" : "status";
            var instances = (await InstanceManagement.ListEc2Instances(config));
            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            if (!string.IsNullOrEmpty(searchString))
            {
                //var searchStringSplit = searchString.Split('=');
                //if (searchStringSplit.Count() > 1)
                //{
                    //searchString = searchStringSplit[1];
                    //var searchType = searchStringSplit[0];
                    switch (CurrentSearchType.ToUpper())
                    {
                        case "IPADDRESS":
                            instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.IpAddress, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                        case "ACCOUNT":
                            instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.AccountName, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                        case "STATUS":
                            instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.Status, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                        case "ID":
                            instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.Id, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                        case "NAME":
                        default:
                            instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.Name, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                    }
                //}
                //else
                //{
                //    instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.Name, searchString, RegexOptions.IgnoreCase).Success).ToList();
                //}
            }

            switch (sortOrder)
            {
                case "name_desc":
                    instances = instances.OrderByDescending(s => s.Name).ToList();
                    break;
                case "status":
                    instances = instances.OrderBy(s => s.Status).ToList();
                    break;
                case "status_desc":
                    instances = instances.OrderByDescending(s => s.Status).ToList();
                    break;
                default:
                    instances = instances.OrderBy(s => s.Name).ToList();
                    break;
            }
            var pageSize = 5;
            Instances = PaginatedList<AwsEc2Instance>.Create(
                instances, pageIndex ?? 1, pageSize);
        }

        public PaginatedList<AwsEc2Instance> Instances { get; set; }
    }
}