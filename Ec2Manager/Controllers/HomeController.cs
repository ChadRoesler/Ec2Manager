using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        public async Task<IActionResult> IndexAsync(string searchtype, string query, int? page, string sortorder)
        {
            _logger.LogInformation("Loading Instances");
            var instances = await InstanceManagement.ListEc2InstancesAsync(_configuration);
            _logger.LogInformation("Initially Loaded with {instances.Count} Instances");
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = _configuration.GetValue<int>("Ec2Manager:ResultsPerPage");
            var search = new SearchService(instances);
            _logger.LogInformation("Passing the following: query = {query} pageNumber = {pageNumber} pageSize = {pageSize} sortOrder = {sortorder}");
            var model = search.GetSearchResult(searchtype, query, pageNumber, pageSize, sortorder);
            _logger.LogInformation("");
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> EnableAsync(string searchtype, string Id, string query, int? page, string sortorder)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var instances = await InstanceManagement.ListEc2InstancesAsync(_configuration);
            var instance = instances.Where(x => x.Id.Equals(Id, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            InstanceManagement.StartEc2Instance(_configuration, instance.Account, instance.Id);
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, sortorder });

        }

        [Authorize]
        public async Task<IActionResult> RebootAsync(string searchtype, string Id, string query, int? page, string sortorder)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var instances = await InstanceManagement.ListEc2InstancesAsync(_configuration);
            var instance = instances.Where(x => x.Id.Equals(Id, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            InstanceManagement.RebootEc2Instance(_configuration, instance.Account, instance.Id);
            return RedirectToAction("Index", new { searchtype, query, page = pageNumber, sortorder });
        }

        [HttpGet]
        public ViewResult Error() => View("Error");

        [HttpGet]
        public ViewResult PageNotFound() => View("PageNotFound");
    }
}
