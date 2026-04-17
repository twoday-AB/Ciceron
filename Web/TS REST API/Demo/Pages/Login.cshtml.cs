using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Net.Http.Headers;

namespace Demo.Pages
{
    public class LoginModel : PageModel
    {
        public string  Message = "";
        private readonly string loginEndpoint = "https://idp.ciceron.cloud/json1.1/Login?customerKey={0}&serviceKey={1}&callbackUrl={2}";
        private readonly string sessionEndpoint = "https://idp.ciceron.cloud/json1.1/GetSession?customerKey={0}&serviceKey={1}&sessionId={2}";
        private readonly string sessionLogout = "https://idp.ciceron.cloud/json1.1/Logout?customerKey={0}&serviceKey={1}&sessionId={2}";
        private readonly IConfiguration config;
        private readonly ILogger<LoginModel> syslog;

        // Setup JSON configuration by appsettings.json for customer and services keys
        public LoginModel(IConfiguration configuration, ILogger<LoginModel> logger)
        {
            config = configuration;
            syslog = logger;
        }

        // Build in handler for HTTP GET /Login

        public async Task<IActionResult> OnGet(string ts_session_id)
        {
            if (ts_session_id == null)
                return await BeginAuthenticate();
            else
                return await EndAuthenticate(ts_session_id);
        }

        // Start the logon processs with twodays magic services
        private async Task<IActionResult> BeginAuthenticate()
        {
            var api = new HttpClient();
            string json = await api.GetStringAsync(String.Format(loginEndpoint, config["customerKey"], config["serviceKey"], Request.GetDisplayUrl()));

             var response = JsonSerializer.Deserialize<Ciceron.Response>(json);

            if (response == null)
                Message = "Technical error";
            else if (response.errorObject != null)
                Message = response.errorObject.message + " (" + response.errorObject.code + ")";
            else
                return Redirect(response.redirectUrl);
         
            return Page();
        }
        
        // End the logon processs and identify the user
        private async Task<IActionResult> EndAuthenticate(string id)
        {
            var api = new HttpClient();
            string json = await api.GetStringAsync(String.Format(sessionEndpoint, config["customerKey"], config["serviceKey"], id));

            var response = JsonSerializer.Deserialize<Ciceron.Response>(json);

            if (response == null)
                Message = "Technical error";
            else if (response.errorObject != null)
                Message = response.errorObject.message + " (" + response.errorObject.code + ")";
            else
                Message = response.userAttributes.serialNumber + " " + response.userAttributes.CN + " " + response.userAttributes.issuerCommonName;
            
            syslog.Log(LogLevel.Information, Message);

            return Page();
        }

        // Kill the session
        private async void Logout(string id)
        {
            var api = new HttpClient();
            string json = await api.GetStringAsync(String.Format(sessionLogout, config["customerKey"], config["serviceKey"], id));

            var response = JsonSerializer.Deserialize<Ciceron.Response>(json);
        }
    }
}
