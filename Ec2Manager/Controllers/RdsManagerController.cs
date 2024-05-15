using Amazon.RDS.Model;
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
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ec2Manager.Controllers
{
    public class RdsManagerController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<HomeController> _logger;


        public RdsManagerController(ILogger<HomeController> logger, IConfiguration configuration)
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
            _logger.LogInformation(string.Format(MessageStrings.LoadingRdsInstances, userClaimPreferredUserNameValue));
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
                claimAccounts.Add(new ClaimValueAccount { Value = "NoClaims", Accounts = AwsRdsManagement.LoadRdsAwsAccounts(_configuration).Select(x => x.AccountName), EnableReboot = _configuration.GetValue<bool>("RdsManager:EnableReboot") });
                claimAccounts.Add(new ClaimValueAccount { Value = "NoClaims", Accounts = AwsRdsManagement.LoadRdsAwsAccounts(_configuration).Select(x => x.AccountName), EnableStop = _configuration.GetValue<bool>("RdsManager:EnableStop") });
            }
            List<RdsInstance> rdsInstances = await AwsRdsManagement.ListRdsInstancesAsync(_configuration, userClaimPreferredUserNameValue);
            _logger.LogInformation(string.Format(MessageStrings.InitialRdsInstanceCount, rdsInstances.Count));
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;
            RdsSearchService search = new(rdsInstances, claimAccounts);
            _logger.LogInformation(string.Format(MessageStrings.SearchedRdsInstanceCount, rdsInstances.Count, searchtype, query, sortorder));
            RdsSearchResult model = search.GetSearchResult(searchtype, query, pageNumber, 5, sortorder);
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> EnableAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            IEnumerable<Claim> userClaims = HttpContext.User.Claims;
            string userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UseRdsrEnable, userClaimPreferredUserNameValue, Id));
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;
            StartDBInstanceResponse response = await AwsRdsManagement.StartRdsInstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserRdsEnableSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RebootAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            IEnumerable<Claim> userClaims = HttpContext.User.Claims;
            string userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserRdsReboot, userClaimPreferredUserNameValue, Id));
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;
            RebootDBInstanceResponse response = await AwsRdsManagement.RebootRdsInstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserRdsRebootSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StopAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            IEnumerable<Claim> userClaims = HttpContext.User.Claims;
            string userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserRdsStop, userClaimPreferredUserNameValue, Id));
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;
            StopDBInstanceResponse response = await AwsRdsManagement.StopRdsInstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserRdsStopSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
