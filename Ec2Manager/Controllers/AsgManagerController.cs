using Ec2Manager.Constants;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Services;
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
    public class AsgManagerController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AsgManagerController> _logger;

        public AsgManagerController(ILogger<AsgManagerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder)
        {
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value ?? ResourceStrings.NoUserName;
            _logger.LogInformation(string.Format(MessageStrings.LoadingAsGroups, userClaimPreferredUserNameValue));
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
                var asgAccounts = AwsAsgManagement.LoadAsgAwsAccounts(_configuration).Select(x => x.AccountName).ToList();
                claimAccounts =
                    [
                        new ClaimValueAccount { Value = "NoClaims", Accounts = asgAccounts, EnableReboot = _configuration.GetValue<bool>("AsgManager:EnableReboot") },
                        new ClaimValueAccount { Value = "NoClaims", Accounts = asgAccounts, EnableStop = _configuration.GetValue<bool>("AsgManager:EnableStop") }
                    ];
            }

            var asGroups = await AwsAsgManagement.ListAsGroupsAsync(_configuration, userClaimPreferredUserNameValue);
            _logger.LogInformation(string.Format(MessageStrings.InitialAsGroupCount, asGroups.Count));
            var pageNumber = page.GetValueOrDefault(1);
            var search = new AsGroupSearchService(asGroups, claimAccounts);
            _logger.LogInformation(string.Format(MessageStrings.SearchedAsGroupCount, asGroups.Count, searchtype, query, sortorder));

            var model = search.GetSearchResult(searchtype, query, pageNumber, 5, sortorder);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> EnableAsync(string Id, string account, int desiredCapacity, int maxCapacity, int minCapacity, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserAsgEnable, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            var response = await AwsAsgManagement.StartGroupAsync(_configuration, userClaimPreferredUserNameValue, account, Id, desiredCapacity, maxCapacity, minCapacity);
            _logger.LogInformation(string.Format(MessageStrings.UserAsgEnableSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RefreashAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserAsgRefresh, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            var response = await AwsAsgManagement.RefreshGroupAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserAsgRefreshSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StopAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaimPreferredUserNameValue = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserAsgStop, userClaimPreferredUserNameValue, Id));

            var pageNumber = page.GetValueOrDefault(1);
            var response = await AwsAsgManagement.StopGroupAsync(_configuration, userClaimPreferredUserNameValue, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserAsgStopSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));

            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
