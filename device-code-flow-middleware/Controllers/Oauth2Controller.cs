using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using device_code_flow_middleware.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace device_code_flow_middleware.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("common/[controller]/v2.0/[action]")]
    public class Oauth2Controller : ControllerBase
    {
        private readonly AppSettingsModel _appSettings;
        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F" };
        private readonly ILogger<Oauth2Controller> _logger;

        public Oauth2Controller(IOptions<AppSettingsModel> appSettings, ILogger<Oauth2Controller> logger)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public string Get()
        {
            return "v0.1";
        }

        [HttpPost]
        public IActionResult DeviceCode()
        {
            // Get the POST data
            StringValues client_id, scope;
            Request.Form.TryGetValue("client_id", out client_id);
            Request.Form.TryGetValue("scope", out scope);

            // Generate user and device codes
            string user_code = StringHelper.GenerateRandomOTP(8, saAllowedCharacters);
            string device_code = StringHelper.GenerateRandomOTP(35, saAllowedCharacters);

            // TBD: change the device ID 
            device_code = StringHelper.Reverse(user_code) + device_code;

            // Retrun the data
            var json = new
            {
                user_code = user_code,
                device_code = device_code,
                verification_uri = _appSettings.VerificationUri,
                expires_in = _appSettings.ExpirationInSeconds,
                interval = _appSettings.IntervalInSeconds,
                message = $"To sign in, use a web browser to open the page {_appSettings.VerificationUri} and enter the code {user_code} to authenticate."
            };

            // Add the new authorization request to the database (async, don't wait)
            TablesService tables = new TablesService(_appSettings.ConnectionString);
            tables.AddAuthRequestAsync(device_code, user_code);

            return new OkObjectResult(json);
        }


        [HttpPost]
        public async Task<IActionResult> TokenAsync()
        {
            // Get the POST data
            StringValues client_id, grant_type, device_code, client_info;
            Request.Form.TryGetValue("client_id", out client_id);
            Request.Form.TryGetValue("client_info", out client_info);
            Request.Form.TryGetValue("grant_type", out grant_type);
            Request.Form.TryGetValue("device_code", out device_code);

            string key = device_code.ToString().Substring(0, 8);
            key = StringHelper.Reverse(key);

            // Check for access token 
            TablesService tables = new TablesService(_appSettings.ConnectionString);
            DeviceEntity device = await tables.GetEntityByUserCodeAsync(key);

            // If the access token exists, then return it to the device
            if (device != null && (device.AccessToken != null))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.ReadToken(device.AccessToken) as JwtSecurityToken;

                var tid = securityToken.Claims.First(claim => claim.Type == "tid").Value;
                var sub = securityToken.Claims.First(claim => claim.Type == "sub").Value;
                var exp = securityToken.Claims.First(claim => claim.Type == "exp").Value;
                var nbf = securityToken.Claims.First(claim => claim.Type == "nbf").Value;

                var json = new
                {
                    token_type = "Bearer",
                    access_token = device.AccessToken,
                    refresh_token = "TBD",
                    id_token = device.AccessToken,
                    expires_in = int.Parse(exp) - int.Parse(nbf),
                    ext_expires_in = int.Parse(exp) - int.Parse(nbf),
                    client_info = StringHelper.Base64Encode("{\"uid\":\"" + sub + "\",\"utid\":\"" + tid + "\"}"),
                    scope = _appSettings.Scope
                };

                // Remove the new authorization request from the database (async, don't wait)
                tables.DeleteEntityByUserCodeAsync(device);

                return new OkObjectResult(json);
            }

            // The user hasn't finished authenticating, but hasn't canceled the flow.
            // Client Action: Repeat the request after at least interval seconds.
            var authorization_pending_response = new
            {
                error = "authorization_pending",
                error_description = $"Error: The authorization request {device_code} is still pending as the user {key} has not yet completed the user interaction steps."
            };

            //TBD: handle the cancel flow

            return new BadRequestObjectResult(authorization_pending_response);

        }
    }
}
