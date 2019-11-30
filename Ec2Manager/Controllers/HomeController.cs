﻿using System.Collections.Generic;
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


        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
        }

        [Authorize]
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder)
        {
            var claimAccounts = new List<ClaimValueAccount>();
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.LoadingInstances, userClaimPreferredUserNameValue));
            if (_configuration.GetValue<string>("OidcAuth:ClientAccountManagementClaim") != null)
            {
                var userClaimAccountManagement = userClaims.Where(x => x.Type == _configuration.GetValue<string>("OidcAuth:ClientAccountManagementClaim") && !Regex.Match(x.Value, "\\[.*\\]", RegexOptions.IgnoreCase).Success);
                var userAccounts = new List<string>();
                userAccounts.AddRange(userClaimAccountManagement.Select(x => x.Value));
                _logger.LogInformation(string.Format(MessageStrings.UserAccountManagment, userClaimPreferredUserNameValue, string.Join(", ", userAccounts)));
                claimAccounts = OpenIdConnectManagement.LoadClaimValueAccounts(_configuration).Where(x => userAccounts.Contains(x.Value)).ToList();
            }
            else
            {
                claimAccounts.Add(new ClaimValueAccount { Value = "NoClaims", Accounts = AwsManagement.LoadAwsAccounts(_configuration).Select(x => x.AccountName), EnableReboot = _configuration.GetValue<bool>("Ec2Manager:EnableReboot") });
            }
            var instances = await AwsManagement.ListEc2InstancesAsync(_configuration);
            _logger.LogInformation(string.Format(MessageStrings.InitialInstanceCount, instances.Count));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var search = new SearchService(instances, claimAccounts);
            _logger.LogInformation(string.Format(MessageStrings.SearchedInstanceCount, instances.Count, searchtype, query, sortorder));
            var model = search.GetSearchResult(searchtype, query, pageNumber, 5, sortorder);
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> EnableAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string  pagesize)
        {
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEnable, userClaimPreferredUserNameValue, Id));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            await AwsManagement.StartEc2InstanceAsync(_configuration, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEnableSuccess, userClaimPreferredUserNameValue, Id));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });

        }

        [Authorize]
        public async Task<IActionResult> RebootAsync(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaims = HttpContext.User.Claims;
            var userClaimPreferredUserNameValue = userClaims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEnable, userClaimPreferredUserNameValue, Id));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            await AwsManagement.RebootEc2InstanceAsync(_configuration, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserRebootSuccess, userClaimPreferredUserNameValue, Id));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
