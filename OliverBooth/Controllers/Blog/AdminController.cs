using System.Net;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Data.Blog;
using OliverBooth.Services;
using ISession = OliverBooth.Data.Blog.ISession;

namespace OliverBooth.Controllers.Blog;

[Controller]
[Route("auth/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IBlogUserService _userService;
    private readonly ISessionService _sessionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AdminController" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="sessionService">The session service.</param>
    public AdminController(ILogger<AdminController> logger,
        IBlogUserService userService,
        ISessionService sessionService)
    {
        _logger = logger;
        _userService = userService;
        _sessionService = sessionService;
    }

    [HttpPost("login")]
    public IActionResult Login()
    {
        string? loginEmail = Request.Form["login-email"];
        string? loginPassword = Request.Form["login-password"];
        IPAddress? remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;

        if (string.IsNullOrWhiteSpace(loginEmail))
        {
            _logger.LogInformation("Login attempt from {Host} with empty login", remoteIpAddress);
            return RedirectToPage("/blog/admin/login");
        }

        if (string.IsNullOrWhiteSpace(loginPassword))
        {
            _logger.LogInformation("Login attempt as '{Email}' from {Host} with empty password", loginEmail,
                remoteIpAddress);
            return RedirectToPage("/blog/admin/login");
        }

        if (_userService.VerifyLogin(loginEmail, loginPassword, out IUser? user))
        {
            _logger.LogInformation("Login attempt for '{Email}' succeeded from {Host}", loginEmail, remoteIpAddress);
        }
        else
        {
            _logger.LogInformation("Login attempt for '{Email}' failed from {Host}", loginEmail, remoteIpAddress);
            return RedirectToPage("/blog/admin/login");
        }

        ISession session = _sessionService.CreateSession(Request, user);
        Span<byte> sessionBytes = stackalloc byte[16];
        session.Id.TryWriteBytes(sessionBytes);
        Response.Cookies.Append("sid", Convert.ToBase64String(sessionBytes));
        return RedirectToPage("/blog/admin/index");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        if (_sessionService.TryGetSession(Request, out ISession? session, true))
            _sessionService.DeleteSession(session);

        Response.Cookies.Delete("sid");
        return RedirectToPage("/blog/admin/login");
    }
}
