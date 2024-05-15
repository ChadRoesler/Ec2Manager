using Amazon.EC2.Model;
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
using System.Security.Claims;

namespace Ec2Manager.Controllers
{
    public class Ec2ManagerController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<HomeController> _logger;


        public Ec2ManagerController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

        }

        [Authorize]
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder)
        {
            List<ClaimValueAccount> claimAccounts = new();
            IEnumerable<Claim> userClaims = HttpContext.User.Claims;
            string userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value ?? ResourceStrings.NoUserName;
            _logger.LogInformation(string.Format(MessageStrings.LoadingEc2Instances, userClaimPreferredUserNameValue));
            if (_configuration.GetValue<string>("OidcAuth:ClientAccountManagementClaim") != null)
            {
                IEnumerable<Claim> userClaimAccountManagement = userClaims.Where(x => x.Type == _configuration.GetValue<string>("OidcAuth:ClientAccountManagementClaim") && !Regex.Match(x.Value, "\\[.*\\]", RegexOptions.IgnoreCase).Success);
                List<string> userAccounts = new();
                userAccounts.AddRange(userClaimAccountManagement.Select(x => x.Value));
                _logger.LogInformation(string.Format(MessageStrings.UserAccountManagment, userClaimPreferredUserNameValue, string.Join(", ", userAccounts)));
                claimAccounts = OpenIdConnectManagement.LoadClaimValueAccounts(_configuration).Where(x => userAccounts.Contains(x.Value)).ToList();
            }
            else
            {
                claimAccounts.Add(new ClaimValueAccount { Value = "NoClaims", Accounts = AwsManagement.LoadEc2AwsAccounts(_configuration).Select(x => x.AccountName), EnableReboot = _configuration.GetValue<bool>("Ec2Manager:EnableReboot") });
                claimAccounts.Add(new ClaimValueAccount { Value = "NoClaims", Accounts = AwsManagement.LoadEc2AwsAccounts(_configuration).Select(x => x.AccountName), EnableStop = _configuration.GetValue<bool>("Ec2Manager:EnableStop") });
            }
            List<Ec2Instance> instances = await AwsManagement.ListEc2InstancesAsync(_configuration, userClaimPreferredUserNameValue);
            _logger.LogInformation(string.Format(MessageStrings.InitialEc2InstanceCount, instances.Count));
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;
            Ec2SearchService search = new(instances, claimAccounts);
            _logger.LogInformation(string.Format(MessageStrings.SearchedEc2InstanceCount, instances.Count, searchtype, query, sortorder));
            Ec2SearchResult model = search.GetSearchResult(searchtype, query, pageNumber, 5, sortorder);
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> EnableAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            IEnumerable<Claim> userClaims = HttpContext.User.Claims;
            string userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UseEc2rEnable, userClaimPreferredUserNameValue, Id));
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;
            StartInstancesResponse response = await AwsManagement.StartEc2InstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEc2EnableSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RebootAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            IEnumerable<Claim> userClaims = HttpContext.User.Claims;
            string userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEc2Reboot, userClaimPreferredUserNameValue, Id));
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;
            RebootInstancesResponse response = await AwsManagement.RebootEc2InstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEc2RebootSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StopAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            IEnumerable<Claim> userClaims = HttpContext.User.Claims;
            string userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEc2Stop, userClaimPreferredUserNameValue, Id));
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;
            StopInstancesResponse response = await AwsManagement.StopEc2InstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEc2StopSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
