using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Ec2Manager.Models;
using Ec2Manager.Workers;
using System;
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

        public async Task OnGetAsync(string sortOrder, string searchString)
        {
            NameSort = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            StatusSort = sortOrder == "status" ? "status_desc" : "status";
            Instances = (await InstanceManagement.ListEc2Instances(config));
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringSplit = searchString.Split('=');
                if (searchStringSplit.Count() > 1)
                {
                    searchString = searchStringSplit[1];
                    var searchType = searchStringSplit[0];
                    switch (searchType.ToUpper())
                    {
                        case "IPADDRESS":
                            Instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.IpAddress, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                        case "ACCOUNT":
                            Instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.AccountName, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                        case "STATUS":
                            Instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.Status, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                        case "ID":
                            Instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.Id, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                        case "NAME":
                        default:
                            Instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.Name, searchString, RegexOptions.IgnoreCase).Success).ToList();
                            break;
                    }
                }
                else
                {
                    Instances = (await InstanceManagement.ListEc2Instances(config)).Where(x => Regex.Match(x.Name, searchString, RegexOptions.IgnoreCase).Success).ToList();
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    Instances = Instances.OrderByDescending(s => s.Name).ToList();
                    break;
                case "status":
                    Instances = Instances.OrderBy(s => s.Status).ToList();
                    break;
                case "status_desc":
                    Instances = Instances.OrderByDescending(s => s.Status).ToList();
                    break;
                default:
                    Instances = Instances.OrderBy(s => s.Name).ToList();
                    break;
            }
        }

        public IList<AwsEc2Instance> Instances { get; set; }
    }
}