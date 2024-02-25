using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Services;

namespace OliverBooth.Pages.Admin;

public class Login : PageModel
{
    private readonly ISessionService _sessionService;

    public Login(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public IActionResult OnGet()
    {
        return _sessionService.TryGetCurrentUser(Request, Response, out _) ? RedirectToPage("/admin/index") : Page();
    }
}
