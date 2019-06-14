using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Ec2Manager.Models;
using Ec2Manager.Workers;

namespace Ec2Manager.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IOptions<AppConfig> config;

        public IndexModel(IOptions<AppConfig> config)
        { 
            this.config = config;
        }

        public async Task OnGetAsync()
        {
            Instances = (await InstanceManagement.ListEc2Instances(config));
        }

        public IList<AwsEc2Instance> Instances { get; set; }
    }
}