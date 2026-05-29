using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace demo_cs_rest;

class Program
{
    public static string Prompt(string prompt, Nullable<ConsoleColor> promptColor = null)
    {
        if (promptColor != null)
        {
            Console.ForegroundColor = promptColor.Value;
            Console.Out.Write(prompt);
            Console.ResetColor();
        }
        else
        {
            Console.Out.Write(prompt);
        }
        return Console.In.ReadLine();
    }

    public static int Main(string[] args)
    {
        AppSettings config = AppSettings.LoadConfig("appsettings.json");

        string host = config.ApiHost;
        string provider = config.Provider;
        string system = config.System;
        string certificateIssuer = config.CertificateIssuer;

        if (host == null || host == "")
        {
            Console.Error.WriteLine("apiHost is not set in appsettings.json. Exiting...");
            return 1;
        }

        while (provider == null || provider == "" || !(provider == "bankid" || provider == "freja"))
        {
            provider = Prompt("Registration provider (freja/bankid): ", ConsoleColor.DarkGray);
        }

        if (system == null || system == "")
        {
            system = Prompt("System: ", ConsoleColor.DarkGray);
        }

        AuthenticationOptions options = new AuthenticationOptions()
        {
            Provider = provider,
            System = system,
            CertificateIssuer = certificateIssuer
        };

        if (provider == "freja")
        {
            // Freja registration level can be either "EXTENDED" or "PLUS". 
            options.MinRegistrationLevel = AuthenticationOptions.MinRegistrationLevelEnum.EXTENDED;
        }

        // personalNumber must have the format described in the following prompt. personalNumber 
        // is not mandatory when communicating with the REST API.
        options.PersonalNumber = Prompt("Personal number (YYYYMMDDNNNN): ", ConsoleColor.DarkGray);
        while (options.PersonalNumber == null || options.PersonalNumber == "" || !Regex.IsMatch(options.PersonalNumber, "^\\d{12}$"))
        {
            options.PersonalNumber = Prompt("Personal number (YYYYMMDDNNNN): ", ConsoleColor.DarkGray);
        }

        // AuthenticationHandler is a wrapper for CertificateServerRequestHandler
        // which sends request corresponding requests to the certificate server.
        using AuthenticationHandler handler = new AuthenticationHandler(host, options);

        // Send a request to start authentication using /rest/auth Returns an orderRef, 
        // infoCode and status. If used with bankid it also returns qrStartToken and 
        // qrStartSecret if personalNumber is not set. 
        var result = handler.Begin();

        Console.Out.WriteLine("Waiting for authentication to complete");

        int retriesOnTimeout = 3;
        AuthenticationPollResult pollResult;
        do
        {
            Thread.Sleep(2000);

            // Poll status of request. Returns status and data of request
            // If request succeeds the endpoint returns data provided by
            // freja or bankid. /rest/auth/collect
            pollResult = handler.Poll();
            Console.Out.WriteLine(pollResult.status + ", " + pollResult.infoCode);

            // If bankid or freja timesout and we want the user to have 
            // more time to verify themselves we have to create a new 
            // token and start polling again. 
            if ((pollResult.infoCode == "expired" || pollResult.infoCode == "requestTimeout") && retriesOnTimeout > 0)
            {
                retriesOnTimeout--;
                result = handler.Begin();
                pollResult = handler.Poll();
            }
            
        } while (pollResult.status == "pending");
        
        Console.Out.WriteLine();

        if (pollResult.status == "complete")
        {
            Console.Out.WriteLine("Success. Hello {0} {1}!", pollResult.givenName, pollResult.surname);
            handler.End(true);
        }
        else
        {
            Console.Out.WriteLine(
                "Authentication failed.\n\tProvider: {0}\n\tSystem: {1}\n\tStatus: {2}\n\tInfo code: {3}",
                options.Provider,
                options.System,
                pollResult.status,
                pollResult.infoCode
            );
            handler.End(true);
        }

        return 0;
    }
}
