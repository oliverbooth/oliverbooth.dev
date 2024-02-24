using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Web;
using OliverBooth.Services;
using ISession = OliverBooth.Data.Blog.ISession;

namespace OliverBooth.Pages.Admin;

public class Index : PageModel
{
    private readonly ILogger<Index> _logger;
    private readonly IUserService _userService;
    private readonly ISessionService _sessionService;

    public Index(ILogger<Index> logger, IUserService userService, ISessionService sessionService)
    {
        _logger = logger;
        _userService = userService;
        _sessionService = sessionService;
    }

    public IUser CurrentUser { get; private set; } = null!;

    public IActionResult OnGet()
    {
        if (!_sessionService.TryGetSession(Request, out ISession? session))
        {
            _logger.LogDebug("Session not found; redirecting");
            return _sessionService.DeleteSessionCookie(Response);
        }

        if (!_sessionService.ValidateSession(Request, session))
        {
            _logger.LogDebug("Session invalid; redirecting");
            return _sessionService.DeleteSessionCookie(Response);
        }

        if (!_userService.TryGetUser(session.UserId, out IUser? user))
        {
            _logger.LogDebug("User not found; redirecting");
            return _sessionService.DeleteSessionCookie(Response);
        }

        CurrentUser = user;
        return Page();
    }
}
