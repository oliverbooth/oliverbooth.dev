using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Common.Services;

namespace OliverBooth.Pages.Admin;

public class MultiFactorStep : PageModel
{
    private readonly ISessionService _sessionService;

    public MultiFactorStep(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public string Token { get; private set; } = string.Empty;

    public int? Result { get; private set; }

    public IActionResult OnGet([FromQuery(Name = "token")] string token,
        [FromQuery(Name = "result")] int? result = null)
    {
        Token = token;
        Result = result;
        return _sessionService.TryGetCurrentUser(Request, Response, out _) ? RedirectToPage("/admin/index") : Page();
    }
}
