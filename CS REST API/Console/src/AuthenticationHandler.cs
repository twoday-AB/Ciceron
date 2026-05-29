using System;

partial class AuthenticationHandler : IDisposable
{
    private readonly CertificateServerRequestHandler Handler;
    private readonly AuthenticationOptions Options;
    private string OrderRef;

    public AuthenticationCancelResult End(bool isDone = false)
    {
        if (isDone)
        {
            OrderRef = null;
        }

        if (OrderRef == null)
        {
            return new AuthenticationCancelResult()
            {
                errorMessage = "Begin was not called.",
                infoCode = "beginNotCalled",
            };
        }

        Console.CancelKeyPress -= HandleConsoleCancel;

        var cancelResult = Handler.CancelAuthentication(OrderRef);
        if (cancelResult.status == "failed")
        {
            Console.Out.WriteLine(
                "Failed to cancel authentication ({0}): {1}",
                cancelResult.infoCode,
                cancelResult.errorMessage
            );
            return cancelResult;
        }

        Console.Out.WriteLine("Authentication cancelled");
        OrderRef = null;

        return cancelResult;
    }

    private void HandleConsoleCancel(object sender, ConsoleCancelEventArgs args)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n===== Interrupt signal recieved. =====");
        Console.ResetColor();
        End();
    }

    public AuthenticationHandler(string serverAddress, AuthenticationOptions options)
    {
        Handler = new CertificateServerRequestHandler(serverAddress, options.CertificateIssuer);
        Options = options;
    }

    public AuthenticationStartResult Begin()
    {
        var authenticationResult = Handler.StartAuthentication(Options);

        OrderRef = authenticationResult.orderRef;
        Console.CancelKeyPress += HandleConsoleCancel;

        return authenticationResult;
    }

    public AuthenticationPollResult Poll()
    {
        if (OrderRef == null)
            return new AuthenticationPollResult()
            {
                status = "failed",
                infoCode = "beginNotCalled",
            };

        return Handler.PollAuthenticationStatus(OrderRef);
    }

    public void Dispose()
    {
        End();
    }
}

