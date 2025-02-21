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
    public class RdsManagerController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<RdsManagerController> _logger;


        public RdsManagerController(ILogger<RdsManagerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

        }

        [Authorize]
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder)
        {
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value ?? ResourceStrings.NoUserName;
            _logger.LogInformation(string.Format(MessageStrings.LoadingRdsInstances, userClaimPreferredUserNameValue));

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
                var rdsAccounts = AwsRdsManagement.LoadRdsAwsAccounts(_configuration).Select(x => x.AccountName).ToList();
                claimAccounts = new List<ClaimValueAccount>
                    {
                        new ClaimValueAccount { Value = "NoClaims", Accounts = rdsAccounts, EnableReboot = _configuration.GetValue<bool>("RdsManager:EnableReboot") },
                        new ClaimValueAccount { Value = "NoClaims", Accounts = rdsAccounts, EnableStop = _configuration.GetValue<bool>("RdsManager:EnableStop") }
                    };
            }

            var rdsObjects = await AwsRdsManagement.ListRdsInstancesAsync(_configuration, userClaimPreferredUserNameValue);
            _logger.LogInformation(string.Format(MessageStrings.InitialRdsInstanceCount, rdsObjects.Count));

            var pageNumber = page.GetValueOrDefault(1);
            var search = new RdsSearchService(rdsObjects, claimAccounts);
            _logger.LogInformation(string.Format(MessageStrings.SearchedRdsInstanceCount, rdsObjects.Count, searchtype, query, sortorder));

            RdsSearchResult model = search.GetSearchResult(searchtype, query, pageNumber, 5, sortorder);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> EnableAsync(string Id, bool isCluster, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserRdsEnable, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            if(isCluster)
            {
                var response = await AwsRdsManagement.StartRdsClusterAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
                _logger.LogInformation(string.Format(MessageStrings.UserRdsEnableSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            }
            else
            {
                var response = await AwsRdsManagement.StartRdsInstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
                _logger.LogInformation(string.Format(MessageStrings.UserRdsEnableSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            }

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RebootAsync(string Id, bool isCluster, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserRdsReboot, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            if (isCluster)
            {
                var response = await AwsRdsManagement.RebootRdsClusterAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
                _logger.LogInformation(string.Format(MessageStrings.UserRdsRebootSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            }
            else
            {
                var response = await AwsRdsManagement.RebootRdsInstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
                _logger.LogInformation(string.Format(MessageStrings.UserRdsRebootSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            }

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StopAsync(string Id, bool isCluster, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserRdsStop, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            if (isCluster)
            {
                var response = await AwsRdsManagement.StopRdsClusterAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
                _logger.LogInformation(string.Format(MessageStrings.UserRdsStopSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            }
            else
            {
                var response = await AwsRdsManagement.StopRdsInstanceAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
                _logger.LogInformation(string.Format(MessageStrings.UserRdsStopSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            }

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
