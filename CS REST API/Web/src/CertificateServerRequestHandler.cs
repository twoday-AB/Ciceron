using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

class CertificateServerRequestHandler
{
    private readonly HttpClient Client;
    public string ServerAddress { get; }

    /// <summary>
    /// A wrapper to perform HTTP requests to Ciceron Certificate Server.
    /// </summary>
    /// <param name="ServerAddress">URL to the certificate server.</param>
    public CertificateServerRequestHandler(string ServerAddress, string certificateIssuer = "")
    {
        if (certificateIssuer == "")
        {
            this.Client = new HttpClient();
        }
        else
        {
            var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2 certificate = store.Certificates.First(cert => cert.IssuerName.Name == certificateIssuer);

            var clientHandler = new HttpClientHandler();
            clientHandler.ClientCertificates.Add(certificate);

            this.Client = new HttpClient(clientHandler);
        }

        Client.BaseAddress = new Uri(ServerAddress);
        this.ServerAddress = ServerAddress;
    }

    /// <summary>
    /// Perform a HTTP request to the /rest/auth endpoint
    /// </summary>
    /// <param name="options">
    /// Contains information about the request. 
    /// "system" and "provider" must not be none in the options.
    /// </param>
    /// <returns>The json result of the request as an object.</returns>
    public AuthenticationStartResult StartAuthentication(AuthenticationOptions options)
    {
        Dictionary<string, string> formData = new Dictionary<string, string>
        {
            {"system", options.System},
            {"provider", options.Provider}
        };

        if (options.PersonalNumber != null)
        {
            formData.Add("personalNumber", options.PersonalNumber);
        }

        if (options.Country != null)
        {
            formData.Add("country", options.Country);
        }

        if (options.MinRegistrationLevel != null)
        {
            formData.Add("minRegistrationLevel", options.MinRegistrationLevel.ToString());
        }

        if (options.CallInitiator != null)
        {
            formData.Add("callInitiator", options.CallInitiator.ToString());
        }

        HttpContent content = new FormUrlEncodedContent(formData);

        Task<HttpResponseMessage> task = Client.PostAsync("/rest/auth", content);
        task.Wait();

        Task<string> jsonContentString = task.Result.Content.ReadAsStringAsync();
        jsonContentString.Wait();

        return JsonSerializer.Deserialize<AuthenticationStartResult>(jsonContentString.Result);
    }

    /// <summary>
    /// Stop ongoing authentication attempt using the /rest/auth/cancel endpoint
    /// </summary>
    /// <param name="orderRef">orderRef that was returned by method Begin</param>
    /// <returns>The json result of the request as an object.</returns>
    public AuthenticationCancelResult CancelAuthentication(string orderRef)
    {
        Dictionary<string, string> formData = new Dictionary<string, string>
        {
            {"orderRef", orderRef}
        };

        HttpContent content = new FormUrlEncodedContent(formData);

        Task<HttpResponseMessage> task = Client.PostAsync("/rest/auth/cancel", content);
        task.Wait();

        Task<string> jsonContentString = task.Result.Content.ReadAsStringAsync();
        jsonContentString.Wait();

        return JsonSerializer.Deserialize<AuthenticationCancelResult>(jsonContentString.Result);
    }

    /// <summary>
    /// Poll status of ongoing authentication request. Uses the /rest/auth/collect endpoint.
    /// </summary>
    /// <param name="orderRef">current request id</param>
    /// <returns>The json result of the request as an object.</returns>
    public AuthenticationPollResult PollAuthenticationStatus(string orderRef)
    {
        Dictionary<string, string> formData = new Dictionary<string, string>
        {
            {"orderRef", orderRef}
        };

        HttpContent content = new FormUrlEncodedContent(formData);

        Task<HttpResponseMessage> task = Client.PostAsync("/rest/auth/collect", content);
        task.Wait();

        Task<string> jsonContentString = task.Result.Content.ReadAsStringAsync();
        jsonContentString.Wait();

        return JsonSerializer.Deserialize<AuthenticationPollResult>(jsonContentString.Result);
    }
}