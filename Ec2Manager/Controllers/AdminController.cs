using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ec2Manager.Constants;
using Ec2Manager.Models.DataManagement;
using Ec2Manager.Models.ConfigManagement;
using Ec2Manager.Workers;

namespace Ec2Manager.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<AdminController> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IEnumerable<ClaimValueAccount> _claimValueAccounts;


        public AdminController(ILogger<AdminController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _claimValueAccounts = OpenIdConnectManagement.LoadClaimValueAccounts(_configuration, _httpContextAccessor);

        }
        [Authorize]
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder)
        {
            var userClaimPreferredUserNameValue = _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value ?? ResourceStrings.NoUserName;
            _logger.LogInformation(string.Format(MessageStrings.LoadingInstances, userClaimPreferredUserNameValue));
            var instances = await AwsManagement.ListEc2InstancesAsync(_configuration);
            _logger.LogInformation(string.Format(MessageStrings.InitialInstanceCount, instances.Count));
            var search = new SearchService(instances, _claimValueAccounts);
            _logger.LogInformation(string.Format(MessageStrings.AdminSearchedInstanceCount, instances.Count, query, sortorder));
            var model = search.GetAdminSearchResult(searchtype, query, sortorder);
            ViewBag.AdminAccess = _claimValueAccounts.SingleOrDefault(x => x.AdminAccess)?.AdminAccess ?? false;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EnableAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEnable, userClaimPreferredUserNameValue, Id));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var response = await AwsManagement.StartEc2InstanceAsync(_configuration, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEnableSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RebootAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserReboot, userClaimPreferredUserNameValue, Id));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var response = await AwsManagement.RebootEc2InstanceAsync(_configuration, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserRebootSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StopAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserStop, userClaimPreferredUserNameValue, Id));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var response = await AwsManagement.StopEc2InstanceAsync(_configuration, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserStopSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");

    }
}