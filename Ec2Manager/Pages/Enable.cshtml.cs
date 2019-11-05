using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Ec2Manager.Models;
using Ec2Manager.Workers;
using Microsoft.AspNetCore.Authorization;

namespace Ec2Manager.Pages
{
    [Authorize]
    public class EnableModel : PageModel
    {
        private readonly IOptions<AppConfig> config;

        public EnableModel(IOptions<AppConfig> config)
        {
            this.config = config;
        }

        [BindProperty]
        public AwsEc2Instance Instance { get; set; }

        public async Task<IActionResult> OnGet(string Id)
        {
            var account = Id.Split('+')[0];
            var instanceId = Id.Split('+')[1];
            await InstanceManagement.StartEc2Instance(config, account, instanceId);
            return RedirectToPage("./Index");
        }
    }
}