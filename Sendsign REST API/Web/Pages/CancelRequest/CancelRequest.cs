using System.Web;
using Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sendsign;
using Sendsign.JsonDocuments;
using Web.Pages.SendSignRequest;

namespace Web.Pages.CancelRequest;

public class SendsignCancelActionPage : PageModel
{
    public readonly SendsignAPICaller apiCaller;

    public string? ApiUrl { get; set; }

    public SendsignCancelActionPage(IConfiguration configuration, ILogger<SendsignSendPage> logger)
    {
        ApiUrl = configuration["ApiUrl"];
        apiCaller = new SendsignAPICaller(ApiUrl ?? "");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var res = apiCaller.CancelSignRequest(new SendsignRequest()
        {
            CustomerKey = HttpContext.Request.Cookies[BaseViewModel.SessionKeyCustomerKey] ?? "",
            Sender = HttpContext.Request.Cookies[BaseViewModel.SessionKeySender] ?? "",
            MessageId = HttpContext.Request.Query["MessageId"]!
        });

        string url = "/SendSignRequest";
        if (res.Error != null)
        {
            url += "?Error=" + HttpUtility.UrlEncode(res.Error);
        }

        return Redirect(url);
    }
}
