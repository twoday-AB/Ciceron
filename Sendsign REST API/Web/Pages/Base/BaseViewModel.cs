using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Base {
    public class BaseViewModel : PageModel
    {
        public const string SessionKeyCustomerKey = "_CustomerKey";
        public const string SessionKeySender = "_Sender";
        public const string SessionKeyMessageIds = "_MessageIds";
        public const string SessionMessageIdIgnore = "_MessageStatus";

        public static readonly CookieOptions CookieOptions = new CookieOptions()
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        };
    }
}