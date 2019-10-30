using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Ec2Manager.Models;
using Ec2Manager.Workers;

namespace Ec2Manager.Pages
{
    public class RebootModel : PageModel
    {
        private readonly IOptions<AppConfig> config;

        public RebootModel(IOptions<AppConfig> config)
        {
            this.config = config;
        }

        [BindProperty]
        public AwsEc2Instance Instance { get; set; }

        public async Task<IActionResult> OnGet(string Id)
        {
            var account = Id.Split('+')[0];
            var instanceId = Id.Split('+')[1];
            await InstanceManagement.RebootEc2Instance(config, account, instanceId);
            return RedirectToPage("./Index");
        }
    }
}