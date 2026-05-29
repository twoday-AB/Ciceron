using Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sendsign;
using Sendsign.JsonDocuments;
using Web.Pages.SendSignRequest;

namespace Web.Pages.RemindSigner;

public class RemindSignerActionPage : PageModel
{
    public readonly SendsignAPICaller apiCaller;

    public string? ApiUrl { get; set; }

    public RemindSignerActionPage(IConfiguration configuration, ILogger<SendsignSendPage> logger)
    {
        ApiUrl = configuration["ApiUrl"];
        apiCaller = new SendsignAPICaller(ApiUrl ?? "");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        apiCaller.SendsignRemindSigner([
            new SendsignRequest()
            {
                CustomerKey = HttpContext.Request.Cookies[BaseViewModel.SessionKeyCustomerKey] ?? "",
                Sender = HttpContext.Request.Cookies[BaseViewModel.SessionKeySender] ?? "",
                MessageId = HttpContext.Request.Query["MessageId"]!
            }
        ]);

        return Redirect("SendSignRequest");
    }
}
