using Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
    public string? CustomerKey { get; set; }

    [BindProperty]
    public string? Sender { get; set; }

    public string? ApiUrl;

    public Dictionary<string, string> StatusMap;

    public IndexModel(IConfiguration configuration)
    {
        try
        {
            ApiUrl = configuration["ApiUrl"];
        }
        catch {}
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (ApiUrl == null || ApiUrl == "")
        {
            return Redirect("MissingAPIEndpoint");
        }

        string? customerKey = HttpContext.Request.Cookies[BaseViewModel.SessionKeyCustomerKey];
        string? sender = HttpContext.Request.Cookies[BaseViewModel.SessionKeySender];
        if (customerKey != null || sender != null)
        {
            CustomerKey = customerKey;
            Sender = sender;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        string? customerKey = HttpContext.Request.Cookies[BaseViewModel.SessionKeyCustomerKey];
        string? sender = HttpContext.Request.Cookies[BaseViewModel.SessionKeySender];
        if (customerKey != null || sender != null)
        {
            CustomerKey = customerKey;
            Sender = sender;
        }
        
        if (CustomerKey == null)
        {
            throw new MissingFieldException("Missing field CustomerKey");
        }

        if (Sender == null)
        {
            throw new MissingFieldException("Missing field Sender");
        }

        HttpContext.Response.Cookies.Append(BaseViewModel.SessionKeyCustomerKey, CustomerKey, BaseViewModel.CookieOptions);
        HttpContext.Response.Cookies.Append(BaseViewModel.SessionKeySender, Sender, BaseViewModel.CookieOptions);

        return Page();
    }
}
