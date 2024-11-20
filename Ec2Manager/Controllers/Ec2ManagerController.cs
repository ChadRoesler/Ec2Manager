using Ec2Manager.Constants;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Models.DataManagement;
using Ec2Manager.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ec2Manager.Controllers
{
    public class Ec2ManagerController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Ec2ManagerController> _logger;

        public Ec2ManagerController(ILogger<Ec2ManagerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder)
        {
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value ?? ResourceStrings.NoUserName;
            _logger.LogInformation(string.Format(MessageStrings.LoadingEc2Instances, userClaimPreferredUserNameValue));

            List<ClaimValueAccount> claimAccounts;
            var clientAccountManagementClaim = _configuration.GetValue<string>("OidcAuth:ClientAccountManagementClaim");

            if (!string.IsNullOrEmpty(clientAccountManagementClaim))
            {
                var userClaimAccountManagement = userClaims
                    .Where(x => x.Type == clientAccountManagementClaim && !Regex.IsMatch(x.Value, "\\[.*\\]", RegexOptions.IgnoreCase))
                    .Select(x => x.Value)
                    .ToList();

                _logger.LogInformation(string.Format(MessageStrings.UserAccountManagment, userClaimPreferredUserNameValue, string.Join(", ", userClaimAccountManagement)));

                claimAccounts = OpenIdConnectManagement.LoadClaimValueAccounts(_configuration)
                    .Where(x => userClaimAccountManagement.Contains(x.Value))
                    .ToList();
            }
            else
            {
                var ec2Accounts = AwsEc2Management.LoadEc2AwsAccounts(_configuration).Select(x => x.AccountName).ToList();
                claimAccounts = new List<ClaimValueAccount>
                    {
                        new ClaimValueAccount { Value = "NoClaims", Accounts = ec2Accounts, EnableReboot = _configuration.GetValue<bool>("Ec2Manager:EnableReboot") },
                        new ClaimValueAccount { Value = "NoClaims", Accounts = ec2Accounts, EnableStop = _configuration.GetValue<bool>("Ec2Manager:EnableStop") }
                    };
            }

            var instances = await AwsEc2Management.ListEc2InstancesAsync(_configuration, userClaimPreferredUserNameValue);
            _logger.LogInformation(string.Format(MessageStrings.InitialEc2InstanceCount, instances.Count));

            var pageNumber = page.GetValueOrDefault(1);
            var search = new Ec2SearchService(instances, claimAccounts);
            _logger.LogInformation(string.Format(MessageStrings.SearchedEc2InstanceCount, instances.Count, searchtype, query, sortorder));

            var model = search.GetSearchResult(searchtype, query, pageNumber, 5, sortorder);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> EnableAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEc2Enable, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            var response = await AwsEc2Management.StartEc2InstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEc2EnableSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RebootAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEc2Reboot, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            var response = await AwsEc2Management.RebootEc2InstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEc2RebootSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StopAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEc2Stop, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            var response = await AwsEc2Management.StopEc2InstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEc2StopSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
