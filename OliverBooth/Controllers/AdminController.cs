using System.Net;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Data.Web;
using OliverBooth.Services;
using ISession = OliverBooth.Data.Web.ISession;

namespace OliverBooth.Controllers;

[Controller]
[Route("auth/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IUserService _userService;
    private readonly ISessionService _sessionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AdminController" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="sessionService">The session service.</param>
    public AdminController(ILogger<AdminController> logger,
        IUserService userService,
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
            return RedirectToPage("/admin/login");
        }

        if (string.IsNullOrWhiteSpace(loginPassword))
        {
            _logger.LogInformation("Login attempt as '{Email}' from {Host} with empty password", loginEmail,
                remoteIpAddress);
            return RedirectToPage("/admin/login");
        }

        if (_userService.VerifyLogin(loginEmail, loginPassword, out IUser? user))
        {
            _logger.LogInformation("Login attempt for '{Email}' succeeded from {Host}", loginEmail, remoteIpAddress);
        }
        else
        {
            _logger.LogInformation("Login attempt for '{Email}' failed from {Host}", loginEmail, remoteIpAddress);
            return RedirectToPage("/admin/login");
        }

        ISession session = _sessionService.CreateSession(Request, user);
        Span<byte> sessionBytes = stackalloc byte[16];
        session.Id.TryWriteBytes(sessionBytes);
        Response.Cookies.Append("sid", Convert.ToBase64String(sessionBytes));
        return RedirectToPage("/admin/index");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        if (_sessionService.TryGetSession(Request, out ISession? session))
            _sessionService.DeleteSession(session);

        return _sessionService.DeleteSessionCookie(Response);
    }
}
