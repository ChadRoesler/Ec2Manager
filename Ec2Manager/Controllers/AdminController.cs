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
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Ec2Manager.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<ClaimValueAccount> _userInfo;


        public AdminController(ILogger<AdminController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userInfo = OpenIdConnectManagement.LoadClaimValueAccounts(_configuration, _httpContextAccessor);

        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder)
        {
            var userClaimPreferredUserNameValue = _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ResourceStrings.UserClaimPreferredUserName)?.Value ?? ResourceStrings.NoUserName;
            _logger.LogInformation(string.Format(MessageStrings.LoadingInstances, userClaimPreferredUserNameValue));
            var instances = await AwsManagement.ListEc2InstancesAsync(_configuration);
            _logger.LogInformation(string.Format(MessageStrings.InitialInstanceCount, instances.Count));
            var search = new SearchService(instances, _userInfo);
            _logger.LogInformation(string.Format(MessageStrings.AdminSearchedInstanceCount, instances.Count, query, sortorder));
            var model = search.GetAdminSearchResult(searchtype, query, sortorder);
            ViewBag.AdminAccess = _userInfo.SingleOrDefault(x => x.AdminAccess)?.AdminAccess ?? false;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Enable(IFormCollection formCollection)
        {
            var instanceIds = formCollection["instancesArray"];
            var distinctInstanceIds = instanceIds.Distinct().Select(x => JsonConvert.DeserializeObject<Ec2InstanceBase>(x)).OrderBy( x => x.Account);
            AwsManagement.AdminStartEc2InstancesAsync(_configuration, distinctInstanceIds);
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reboot(IFormCollection formCollection)
        {
            var instanceIds = formCollection["instancesArray"];
            var distinctInstanceIds = instanceIds.Distinct().Select(x => JsonConvert.DeserializeObject<Ec2InstanceBase>(x)).OrderBy(x => x.Account);
            AwsManagement.AdminRebootEc2InstancesAsync(_configuration, distinctInstanceIds);
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Stop(IFormCollection formCollection)
        {
            var instanceIds = formCollection["instancesArray"];
            var distinctInstanceIds = instanceIds.Distinct().Select(x => JsonConvert.DeserializeObject<Ec2InstanceBase>(x)).OrderBy(x => x.Account);
            AwsManagement.AdminStopEc2InstancesAsync(_configuration, distinctInstanceIds);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");

    }
}