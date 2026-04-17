using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Pages
{
    public class SignModel : PageModel
    {
        public string Text = "Jag godkänner avtalet 12345";
        public string Hidden = "<agreement id='12345'><data></data></agreement>";
        public string Message = "";
        public string Issuer = "";
        public string Digest = "";
        public string Signature = "";
        public string TransactionID = "";
        public string Timestamp = "";
        public string CommonName = "";
        private readonly string signEndpoint = "https://idp.ciceron.cloud/json1.1/Sign?customerKey={0}&serviceKey={1}&callbackUrl={2}&userVisibleData={3}&userNonVisibleData={4}";
        private readonly string sessionEndpoint = "https://idp.ciceron.cloud/json1.1/GetSession?customerKey={0}&serviceKey={1}&sessionId={2}&logout=true";
        private readonly IConfiguration config;
        private readonly ILogger<SignModel> syslog;


        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        // Setup JSON configuration by appsettings.json for my security keys
        public SignModel(IConfiguration configuration, ILogger<SignModel> logger)
        {
            config = configuration;
            syslog = logger;
        }

        // Build in handler for HTTP GET /Sign
        public async Task<IActionResult> OnGet(string ts_session_id)
        {
            if (ts_session_id == null)
                return await BeginSign();
            else
                return await EndSign(ts_session_id, "true");
        }

        // Start the sign processs with twodays magic services
        private async Task<IActionResult> BeginSign()
        {
            var api = new HttpClient();
            string json = await api.GetStringAsync(String.Format(signEndpoint, config["customerKey"], config["serviceKey"], Request.GetDisplayUrl(), Base64Encode(Text), Base64Encode(Hidden)));

            var response = JsonSerializer.Deserialize<Ciceron.Response>(json);

            if (response == null)
                Message = "Technical error";
            else if (response.errorObject != null)
                Message = response.errorObject.message + " (" + response.errorObject.code + ")";
            else
                return Redirect(response.redirectUrl);

            return Page();
        }

        // End the sign processs and store the legal electronic signature
        private async Task<IActionResult> EndSign(string id, string reset)
        {
            var api = new HttpClient();
            string json = await api.GetStringAsync(String.Format(sessionEndpoint, config["customerKey"], config["serviceKey"], id, reset));

            var response = JsonSerializer.Deserialize<Ciceron.Response>(json);

            if (response == null)
                Message = "Technical error";
            else if (response.errorObject != null)
                Message = response.errorObject.message + " (" + response.errorObject.code + ")";
            else
            {
                Message = response.userAttributes.SignMessage;
                Timestamp = response.userAttributes.Timestamp;
                Digest = response.userAttributes.SignDigest;
                Signature = response.userAttributes.Signature;
                TransactionID = response.userAttributes.TransactionId;
                CommonName = response.userAttributes.CN;
                Issuer = response.userAttributes.issuerCommonName;
            }

            syslog.Log(LogLevel.Information, Message);

            return Page();
        }
    }
}
