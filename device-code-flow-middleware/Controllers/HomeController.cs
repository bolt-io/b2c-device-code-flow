using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using device_code_flow_middleware.Models;
using Microsoft.Identity.Web;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace device_code_flow_middleware.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    public class HomeController : Controller
    {

        private readonly AppSettingsModel _appSettings;
        private readonly ILogger<HomeController> _logger;
        readonly ITokenAcquisition _tokenAcquisition;

        public HomeController(IOptions<AppSettingsModel> appSettings, ITokenAcquisition tokenAcquisition, ILogger<HomeController> logger)
        {
            _logger = logger;
            _tokenAcquisition = tokenAcquisition;
            _appSettings = appSettings.Value;
        }


        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    // Get the scopes
                    string[] scopes =  _appSettings.Scope.Split(",");

                    string token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                    string userCode = string.Empty;

                    foreach (Claim claim in this.User.Claims)
                    {
                        // Check the user code and update the database
                        if (claim.Type == "userCode")
                        {
                            userCode = claim.Value;

                            // Update the request with the access token (async, don't wait)
                            TablesService tables = new TablesService(_appSettings.ConnectionString);
                            tables.UpdateAccessTokenAsync(userCode, token);

                            break;
                        }
                    }

                    Console.WriteLine(userCode);
                }
                catch (Exception e)
                {
                    //  Block of code to handle errors
                }

            }

            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
