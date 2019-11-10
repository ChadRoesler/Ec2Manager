using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ec2Manager.Constants;
using Ec2Manager.Models.DataManagement;
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
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder, string  pagesize)
        {
            var userClaims = HttpContext.User.Claims;
            var username = userClaims.Where(x => x.Type == ResourceStrings.UserClaimUserName).FirstOrDefault().Value;
            _logger.LogInformation(string.Format(MessageStrings.LoadingInstances, username));
            var instances = await InstanceManagement.ListEc2InstancesAsync(_configuration);
            _logger.LogInformation(string.Format(MessageStrings.InitialInstanceCount, instances.Count));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var search = new SearchService(instances);
            _logger.LogInformation(string.Format(MessageStrings.SearchedInstanceCount, instances.Count, searchtype, query, sortorder));
            var model = search.GetSearchResult(searchtype, query, pageNumber, pagesize, sortorder);
            return View(model);
        }


        [Authorize]
        public IActionResult Enable(string Id, string account, string searchtype, string query, int? page, string sortorder, string  pagesize)
        {
            var userClaims = HttpContext.User.Claims;
            var username = userClaims.Where(x => x.Type == ResourceStrings.UserClaimUserName).FirstOrDefault().Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEnable, username, Id));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            InstanceManagement.StartEc2Instance(_configuration, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserEnableSuccess, username, Id));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });

        }

        [Authorize]
        public IActionResult Reboot(string Id, string account, string searchtype, string query, int? page, string sortorder, string pagesize)
        {
            var userClaims = HttpContext.User.Claims;
            var username = userClaims.Where(x => x.Type == ResourceStrings.UserClaimUserName).FirstOrDefault().Value;
            _logger.LogInformation(string.Format(MessageStrings.UserEnable, username, Id));
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            InstanceManagement.RebootEc2Instance(_configuration, account, Id);
            _logger.LogInformation(string.Format(MessageStrings.UserRebootSuccess, username, Id));
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, pagesize, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
