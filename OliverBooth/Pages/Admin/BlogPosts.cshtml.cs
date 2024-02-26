using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Web;
using OliverBooth.Services;

namespace OliverBooth.Pages.Admin;

public class BlogPosts : PageModel
{
    private readonly ISessionService _sessionService;

    public BlogPosts(ISessionService sessionService)
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
