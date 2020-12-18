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
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<ClaimValueAccount> _userInfo;
        private readonly SearchData searchData;


        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userInfo = OpenIdConnectManagement.LoadClaimValueAccounts(_configuration, _httpContextAccessor);
            searchData = new SearchData();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder, int pagesize)
        {
            searchData.SortOrder = sortorder;
            searchData.SearchQuery = query;
            searchData.PageSize = pagesize;
            searchData.Page = page ?? 1;
            //searchData.SortOrder = searchdata.SortOrder;
            //searchData.Page = searchdata.Page;
            //searchData.PageSize = searchdata.PageSize;
            //searchData.SearchQuery = searchdata.SearchQuery;
            var claimAccounts = new List<ClaimValueAccount>();
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value ?? ResourceStrings.NoUserName;
            _logger.LogInformation(string.Format(MessageStrings.LoadingInstances, userClaimPreferredUserNameValue));
            var instances = await AwsManagement.ListEc2InstancesAsync(_configuration);
            _logger.LogInformation(string.Format(MessageStrings.InitialInstanceCount, instances.Count));
            var search = new SearchService(instances, _userInfo);
            _logger.LogInformation(string.Format(MessageStrings.SearchedInstanceCount, instances.Count, searchData.SearchType, searchData.SearchQuery, searchData.SortOrder));
            var model = search.GetSearchResult(searchData);
            ViewBag.AdminAccess = _userInfo.SingleOrDefault(x => x.AdminAccess)?.AdminAccess ?? false;
            return View(model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EnableAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string  pagesize)
        {
            try
            {
                var userClaims = HttpContext.User.Claims;
                var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
                _logger.LogInformation(string.Format(MessageStrings.UserEnable, userClaimPreferredUserNameValue, Id));
                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var response = await AwsManagement.StartEc2InstanceAsync(_configuration, account, Id);
                _logger.LogInformation(string.Format(MessageStrings.UserEnableSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
                return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
            }
            catch
            {
                return View("Error");
            }

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RebootAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            try
            { 
                if (_userInfo.SingleOrDefault(x => x.EnableReboot && x.Accounts.Contains(account)) != null)
                {
                    var userClaims = HttpContext.User.Claims;
                    var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
                    _logger.LogInformation(string.Format(MessageStrings.UserReboot, userClaimPreferredUserNameValue, Id));
                    var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                    var response = await AwsManagement.RebootEc2InstanceAsync(_configuration, account, Id);
                    _logger.LogInformation(string.Format(MessageStrings.UserRebootSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
                    return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
                }
                else
                {
                    throw new System.Exception();
                }
            }
            catch
            {
                return View("Error");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StopAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            try
            {
                if (_userInfo.SingleOrDefault(x => x.EnableStop && x.Accounts.Contains(account)) != null)
                {
                    var userClaims = HttpContext.User.Claims;
                    var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
                    _logger.LogInformation(string.Format(MessageStrings.UserStop, userClaimPreferredUserNameValue, Id));
                    var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                    var response = await AwsManagement.StopEc2InstanceAsync(_configuration, account, Id);
                    _logger.LogInformation(string.Format(MessageStrings.UserStopSuccess, userClaimPreferredUserNameValue, Id, response.HttpStatusCode.ToString()));
                    return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
                }
                else
                {
                    throw new System.Exception();
                }
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
