using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ec2Manager.Models;
using Microsoft.Extensions.Configuration;
using Ec2Manager.Workers;
using Microsoft.AspNetCore.Authorization;
using Ec2Manager.Models.DataManagement;
using Ec2Manager.Interfaces;

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
        public async Task<IActionResult> IndexAsync(string query, int? page)
        {
            var instances = await InstanceManagement.ListEc2InstancesAsync(_configuration);
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = _configuration.GetValue<int>("Ec2Manager:ResultsPerPage");
            var search = new SearchService(instances);

            var model = search.GetSearchResult(query, pageNumber, pageSize);
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> EnableAsync(string Id, string query, int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var instances = await InstanceManagement.ListEc2InstancesAsync(_configuration);
            var instance = instances.Where(x => x.Id.Equals(Id, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            InstanceManagement.StartEc2Instance(_configuration, instance.Account, instance.Id);
            return RedirectToAction("Index", new {query, page = pageNumber });
        }

        [Authorize]
        public async Task<IActionResult> RebootAsync(string Id, string query, int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var instances = await InstanceManagement.ListEc2InstancesAsync(_configuration);
            var instance = instances.Where(x => x.Id.Equals(Id, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            InstanceManagement.RebootEc2Instance(_configuration, instance.Account, instance.Id);
            return RedirectToAction("Index", new { query, page = pageNumber });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
