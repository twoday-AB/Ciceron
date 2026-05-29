using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sendsign.JsonDocuments;
using System.Text.Json;
using Sendsign;
using Base;

namespace Web.Pages.SendSignRequest;

public class SendsignSendPage : PageModel
{
    public const string SessionKeyRecipientCount = "_RecipientCount";

    [BindProperty]
    public IFormFile[]? Files { get; set; }

    [BindProperty]
    public SendsignSendRequest? SendsignSendRequestInstance { get; set; }

    [BindProperty]
    public SendsignRecipient[] SendsignRecipients { get; set; } = [new SendsignRecipient()];

    [BindProperty]
    public int? RecipientCount { get; set; }

    public readonly SendsignAPICaller apiCaller;
    private readonly IConfiguration configuration;
    private readonly ILogger<SendsignSendPage> logger;

    public string? LastMessageID;

    public string GetCustomerKey()
    {
        return HttpContext.Request.Cookies[BaseViewModel.SessionKeyCustomerKey] ?? "";
    }

    public string GetSender()
    {
        return HttpContext.Request.Cookies[BaseViewModel.SessionKeySender] ?? "";
    }

    public string? ApiUrl { get; set; }

    public SendsignSendPage(IConfiguration configuration, ILogger<SendsignSendPage> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
        ApiUrl = configuration["ApiUrl"];
        apiCaller = new SendsignAPICaller(ApiUrl ?? "");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        InitPage();
        return Page();
    }

    public void InitPage()
    {
        SendsignRecipients = new SendsignRecipient[HttpContext.Session.GetInt32(SessionKeyRecipientCount) ?? 1];

        if (Request.Query["action"] == "ClearMessageIds")
        {
            ClearMessageIds();
        }
    }

    public void ClearMessageIds()
    {
        Response.Cookies.Delete(BaseViewModel.SessionKeyMessageIds);
    }

    public static List<T> CookieToArray<T>(HttpContext context, string key)
    {
        return JsonSerializer.Deserialize<List<T>>(context.Request.Cookies[key] ?? "[]") ?? [];
    }

    public static void StoreCookie<T>(HttpContext context, List<T> values, string key)
    {
        context.Response.Cookies.Append(key, JsonSerializer.Serialize(values));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        InitPage();

        for (int i = 0; i < SendsignRecipients.Length; i++)
        {
            SendsignRecipients[i] = new SendsignRecipient
            {
                Mail = Request.Form[$"SendsignRecipients[{i}].Mail"]!,
                Name = Request.Form[$"SendsignRecipients[{i}].Name"]!,
                Sms = Request.Form[$"SendsignRecipients[{i}].Sms"]!,
                Ssn = Request.Form[$"SendsignRecipients[{i}].Ssn"]!,
                Type = Request.Form[$"SendsignRecipients[{i}].Type"]!
            };
        }

        if (RecipientCount != null)
        {
            if (RecipientCount <= 0)
            {
                RecipientCount = 1;
            }
            else if (RecipientCount > 20)
            {
                RecipientCount = 20;
            }

            HttpContext.Session.SetInt32(SessionKeyRecipientCount, (int) RecipientCount);
            SendsignRecipients = new SendsignRecipient[(int) RecipientCount];
            return Page();
        }

        if (Files == null || Files.Length == 0)
        {
            throw new MissingFieldException("No files were uploaded");
        }

        if (SendsignSendRequestInstance == null)
        {
            throw new MissingFieldException("Sendsign recipient is null");
        }

        if (SendsignRecipients == null)
        {
            throw new MissingFieldException("Sendsign recipient is null");
        }

        foreach (var file in Files)
        {
            Stream stream = file.OpenReadStream();
            byte[] bytes = new byte[file.Length];
            stream.Read(bytes);
            
            var b64 = Convert.ToBase64String(bytes);
            SendsignSendRequestInstance.Attachments.Add(new SendsignAttachment()
            {
                ContentType = "application/pdf",
                Data = b64,
                Name = file.FileName
            });
        }
        
        SendsignSendRequestInstance.Recipients = SendsignRecipients;
        SendsignSendRequestInstance.CustomerKey = HttpContext.Request.Cookies[BaseViewModel.SessionKeyCustomerKey] ?? "";
        SendsignSendRequestInstance.Sender = HttpContext.Request.Cookies[BaseViewModel.SessionKeySender] ?? "";
        SendsignSendResponse resp = apiCaller.Send(this.SendsignSendRequestInstance)!;

        var json = CookieToArray<string>(HttpContext, BaseViewModel.SessionKeyMessageIds);
        if (resp.MessageSent != null)
            json.Add(resp.MessageSent);
        else
            logger.LogError(resp.Error);

        HttpContext.Response.Cookies.Append(BaseViewModel.SessionKeyMessageIds, JsonSerializer.Serialize(json), BaseViewModel.CookieOptions);
        LastMessageID = resp.MessageSent;

        return Page();
    }
}
