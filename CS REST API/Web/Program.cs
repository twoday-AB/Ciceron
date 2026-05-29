using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/", (HttpResponse response) => response.Redirect("index.html"));

var handlers = new Dictionary<string, AuthenticationHandler>();
const int RETRIES = 10; // 10 * 30s = 5min
int retriesLeft = RETRIES;

const string HTML_TEMPLATE = """
    <!doctype html>
    <html lang=en>
    <meta charset=utf-8>
    <meta name=viewport content="width=device-width; initial-scale=1.0">
    <title> {0} - Demo CS REST API</title>
    <link rel="shortcut icon" href="twoday-favicon.svg">
    <style>
        :root {{ color-scheme: light dark }}
    </style>
    <main>
        {1}
    </main>
    """;
static string html(string title, string content)
{
    return string.Format(HTML_TEMPLATE, title, content);  
}

// auth, poll, cancel
app.MapPost("/auth", async context =>
{
    IFormCollection form = await context.Request.ReadFormAsync();
    if (!form.TryGetValue("apiHost", out StringValues apiHostValue)
        || apiHostValue.Count != 1
        || string.IsNullOrWhiteSpace(apiHostValue.ToString())
        || !form.TryGetValue("provider", out StringValues providerValue)
        || providerValue.Count != 1
        || string.IsNullOrWhiteSpace(providerValue.ToString())
        || !form.TryGetValue("system", out StringValues systemValue)
        || systemValue.Count != 1
        || string.IsNullOrWhiteSpace(systemValue.ToString())
       )
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Missing, multiple or empty required fields.");
        return;
    }
    string apiHost = apiHostValue.ToString();
    try
    {
        var uri = new Uri(apiHost, UriKind.Absolute);
    }
    catch (UriFormatException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Bad uri format api host.");
        return;
    }
    string provider = providerValue.ToString();
    string system = systemValue.ToString();
    string personalNumber;
    if (!form.TryGetValue("personalNumber", out StringValues personalNumberValue)
        || personalNumberValue.Count != 1
        || string.IsNullOrWhiteSpace(personalNumberValue.ToString()))
    {
        personalNumber = "";
    }
    else
    {
        personalNumber = personalNumberValue.ToString();
    }
    string certificateIssuer;
    if (!form.TryGetValue("certificateIssuer", out StringValues certificateIssuerValue)
        || certificateIssuerValue.Count != 1
        || string.IsNullOrWhiteSpace(certificateIssuerValue.ToString()))
    {
        certificateIssuer = "";
    }
    else
    {
        certificateIssuer = certificateIssuerValue.ToString();
    }

    var options = new AuthenticationOptions
    {
        Provider = provider,
        System = system,
        CertificateIssuer = certificateIssuer,
        PersonalNumber = personalNumber,
    };

    if (provider == "freja")
    {
        options.MinRegistrationLevel = AuthenticationOptions.MinRegistrationLevelEnum.EXTENDED;
    }

    var handler = new AuthenticationHandler(apiHost, options);
    AuthenticationStartResult begin = handler.Begin();
    
    if (begin.status != "pending")
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("Failed to begin.");
        return;
    }

    handlers.Add(begin.orderRef, handler);
    Interlocked.Exchange(ref retriesLeft, RETRIES);

    context.Response.Redirect("/poll?orderRef="+begin.orderRef);
});


app.MapGet("/poll", async context =>
{
    if (!context.Request.Query.TryGetValue("orderRef", out StringValues orderRefValue)
        || orderRefValue.Count != 1
        || string.IsNullOrWhiteSpace(orderRefValue.ToString()))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Missing, multiple or empty required orderRef query string.");
        return;
    }
    string orderRef = context.Request.Query["orderRef"].ToString();

    var handler = handlers[orderRef];
    AuthenticationPollResult result = handler.Poll();

    if (result.status == "pending")
    {
        context.Response.Headers["Refresh"] = "2";

        await context.Response.WriteAsync(html("In Progress", $"""
        <pre>Auth in progress. 
        Polling every 2 sec. 
        order ref.: {orderRef}
        status:     {result.status}
        info code:  {result.infoCode}
        </pre>
        <form action=cancel method=post>
            <input type=hidden name=orderRef value={orderRef}>
            <input type=submit value=Cancel>
        </form>
        """));
    }
    else if (result.status == "complete")
    {
        handlers.Remove(orderRef);

        await context.Response.WriteAsync(html("Completed", $"""
            <pre>Completed
            =========================
            status:          {result.status}
            info code:       {result.infoCode}
            personal number: {result.personalNumber}
            given name:      {result.givenName}
            surname:         {result.surname}
            email:           {result.email}
            cert not before: {result.certNotBefore} (BankId only)
            cert not after:  {result.certNotAfter} (BankId only)</pre>
            <a href=/>Home</a>
            """));
    }
    else if (result.status == "failed" &&
             (result.infoCode == "expired" ||  result.infoCode == "requestTimeout") &&
             retriesLeft > 0)
    {
        handlers.Remove(orderRef);

        AuthenticationStartResult begin = handler.Begin();
        
        if (begin.status != "pending")
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Failed to begin.");
            return;
        }

        handlers.Add(begin.orderRef, handler);
        Interlocked.Decrement(ref retriesLeft);

        // Immediate redirect.
        //context.Response.Redirect("/poll?orderRef=" + begin.orderRef);
        //return;


        context.Response.Headers["Refresh"] = $"5; URL=/poll?orderRef={begin.orderRef}"; // Not accesible
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync(html("Timeout", $"""
            <pre>Timeout, continuing with new request up to 10 times.
            Redirect in 5 seconds...

            =========================
            status:          {result.status}
            info code:          {result.infoCode}</pre>
            """));
    }
    else if (result.status == "failed")
    {
        handlers.Remove(orderRef);
        string outOfRetires = (retriesLeft < 1) ? "(no more retries)" : "";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(html("Failed", $"""
            <pre>Failed {outOfRetires}
            =========================
            status:          {result.status}
            info code:          {result.infoCode}</pre>
            <a href=/>Home</a>
            """));
    }
    else if (result.status == "error")
    {
        handlers.Remove(orderRef);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(html("Error", $"""
            <pre>Error
            =========================
            status:          {result.status}
            info code:          {result.infoCode}</pre>
            <a href=/>Home</a>
            """));
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(html("Error", $"""
            <pre>Invalid poll status.
            status: {result.status}
            info code: {result.infoCode}</pre>
            <a href=/>Home</a>
            """));
    }
});

app.MapPost("/cancel", async context =>
{
    IFormCollection form = await context.Request.ReadFormAsync();
    if (!form.TryGetValue("orderRef", out StringValues orderRefValue)
        || orderRefValue.Count != 1
        || string.IsNullOrWhiteSpace(orderRefValue.ToString()))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Missing, multiple or empty required orderRef value.");
        return;
    }
    string orderRef = orderRefValue.ToString();

    if (!handlers.TryGetValue(orderRef, out var handler))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Invalid orderRef value.");
        return;
    }
   
    AuthenticationCancelResult result = handler.End();

    if (result.status != "cancelled")
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(html("Cancel failed", $"""
            <pre>Order cancel failed
            ======================================
            orderRef:      {orderRef}
            status:        {result.status}
            errorMessage:  {result.errorMessage}</pre>
            <a href=/>Home</a>
            """));
        return;
    }

    handlers.Remove(orderRef);
    await context.Response.WriteAsync(html("Cancel", $"""
        <pre>Order cancelled
        ======================================
        orderRef:  {orderRef}
        status:    {result.status}
        infoCode:  {result.infoCode}</pre>
        <a href=/>Home</a>
        """));
    return;
});


app.Run();
