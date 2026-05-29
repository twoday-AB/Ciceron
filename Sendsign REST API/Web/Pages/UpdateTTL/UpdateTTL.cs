using Base;
using Sendsign;
using System.Web;
using Sendsign.JsonDocuments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Pages.SendSignRequest;

namespace Web.Pages.UpdateTTL;

public class UpdateTTLActionPage : PageModel
{
    public readonly SendsignAPICaller apiCaller;

    public string? ApiUrl { get; set; }

    [BindProperty]
    public int? TimeToLive { get; set; }

    [BindProperty]
    public string? MessageId { get; set; }

    public UpdateTTLActionPage(IConfiguration configuration, ILogger<SendsignSendPage> logger)
    {
        ApiUrl = configuration["ApiUrl"];
        apiCaller = new SendsignAPICaller(ApiUrl ?? "");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (MessageId == null)
        {
            throw new MissingFieldException("Missing field MessageId");
        }

        if (TimeToLive == null)
        {
            throw new MissingFieldException("Missing field TimeToLive");
        }

        var res = apiCaller.UpdateTTLForMessage(
            new SendsignUpdateTTLRequest()
            {
                CustomerKey = HttpContext.Request.Cookies[BaseViewModel.SessionKeyCustomerKey] ?? "",
                Sender = HttpContext.Request.Cookies[BaseViewModel.SessionKeySender] ?? "",
                MessageId = MessageId,
                TTL = Convert.ToString(TimeToLive)
            }
        );

        string url = "/SendSignRequest";
        if (res.Error != null)
        {
            url += "?Error=" + HttpUtility.UrlEncode(res.Error);
        }

        return Redirect(url);
    }
}
