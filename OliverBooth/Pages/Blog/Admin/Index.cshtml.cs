using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Blog;
using OliverBooth.Services;
using ISession = OliverBooth.Data.Blog.ISession;

namespace OliverBooth.Pages.Blog.Admin;

public class Index : PageModel
{
    private readonly IBlogUserService _userService;
    private readonly ISessionService _sessionService;

    public Index(IBlogUserService userService, ISessionService sessionService)
    {
        _userService = userService;
        _sessionService = sessionService;
    }

    public IUser CurrentUser { get; private set; } = null!;

    public IActionResult OnGet()
    {
        IPAddress? remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null)
        {
            return RedirectToPage("login");
        }

        if (!Request.Cookies.TryGetValue("sid", out string? sessionIdCookie))
        {
            return RedirectToPage("login");
        }

        Span<byte> bytes = stackalloc byte[16];
        if (!Convert.TryFromBase64Chars(sessionIdCookie, bytes, out int bytesWritten) || bytesWritten < 16)
        {
            Response.Cookies.Delete("sid");
            return RedirectToPage("login");
        }

        var sessionId = new Guid(bytes);
        if (!_sessionService.TryGetSession(sessionId, out ISession? session))
        {
            Response.Cookies.Delete("sid");
            return RedirectToPage("login");
        }

        if (session.Expires <= DateTimeOffset.UtcNow)
        {
            _sessionService.DeleteSession(session);
            Response.Cookies.Delete("sid");
            return RedirectToPage("login");
        }

        Span<byte> remoteAddressBytes = stackalloc byte[16];
        Span<byte> sessionAddressBytes = stackalloc byte[16];
        if (!remoteIpAddress.TryWriteBytes(remoteAddressBytes, out _) ||
            !session.IpAddress.TryWriteBytes(sessionAddressBytes, out _))
        {
            Response.Cookies.Delete("sid");
            return RedirectToPage("login");
        }

        if (!remoteAddressBytes.SequenceEqual(sessionAddressBytes))
        {
            Response.Cookies.Delete("sid");
            return RedirectToPage("login");
        }

        if (!_userService.TryGetUser(session.UserId, out IUser? user))
        {
            Response.Cookies.Delete("sid");
            return RedirectToPage("login");
        }

        CurrentUser = user;
        return Page();
    }
}
