using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Common.Data.Web.Users;
using OliverBooth.Common.Services;

namespace OliverBooth.Pages.Admin;

public class Index : PageModel
{
    private readonly ISessionService _sessionService;

    public Index(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public IUser CurrentUser { get; private set; } = null!;

    public IActionResult OnGet()
    {
        if (!_sessionService.TryGetCurrentUser(Request, Response, out IUser? user))
        {
            return RedirectToPage("/admin/login");
        }

        CurrentUser = user;
        return Page();
    }
}
