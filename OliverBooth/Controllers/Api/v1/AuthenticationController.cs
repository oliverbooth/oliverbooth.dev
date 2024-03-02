using System.Diagnostics;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Common.Data.Web.Users;
using OliverBooth.Common.Services;
using ISession = OliverBooth.Common.Data.Web.Users.ISession;

namespace OliverBooth.Controllers.Api.v1;

/// <summary>
///     Represents a controller which handles user authentication.
/// </summary>
[Controller]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion(1)]
public sealed class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly ISessionService _sessionService;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthenticationController" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="sessionService">The session service.</param>
    /// <param name="userService">The user service.</param>
    public AuthenticationController(ILogger<AuthenticationController> logger,
        ISessionService sessionService,
        IUserService userService)
    {
        _logger = logger;
        _sessionService = sessionService;
        _userService = userService;
    }

    /// <summary>
    ///     Authorizes a multi-factor login request using the specified token and TOTP.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="totp">The time-based one-time password.</param>
    /// <returns>The result of the authentication process.</returns>
    public IActionResult DoMultiFactor([FromForm(Name = "token")] string token, [FromForm(Name = "totp")] string totp)
    {
        string epName = nameof(DoMultiFactor);
        if (Request.HttpContext.Connection.RemoteIpAddress is not { } ip)
        {
            _logger.LogWarning("Endpoint {Name} reached with no remote IP!", epName);
            return BadRequest();
        }

        MfaRequestResult result = _userService.VerifyMfaRequest(token, totp, out IUser? user);
        switch (result)
        {
            case MfaRequestResult.InvalidTotp:
                return RedirectToPage("/admin/multifactorstep", new { token, result = (int)result });

            case MfaRequestResult.TokenExpired:
                return RedirectToPage("/admin/login", new { result = (int)result });

            case MfaRequestResult.TooManyAttempts:
                return RedirectToPage("/admin/login", new { result = (int)result });
        }

        Debug.Assert(user is not null);

        _userService.DeleteToken(token);

        ISession session = _sessionService.CreateSession(Request, user);
        _sessionService.SaveSessionCookie(Response, session);
        _logger.LogInformation("MFA request from {Host} with login {Login} succeeded", ip, user.EmailAddress);
        return RedirectToPage("/admin/index");
    }

    /// <summary>
    ///     Authorizes a login request using the specified credentials.
    /// </summary>
    /// <param name="emailAddress">The login email address.</param>
    /// <param name="password">The login password.</param>
    /// <returns>The result of the authentication process.</returns>
    [HttpPost("signin")]
    public IActionResult DoSignIn([FromForm(Name = "login-email")] string emailAddress,
        [FromForm(Name = "login-password")] string password)
    {
        string epName = nameof(DoSignIn);
        if (Request.HttpContext.Connection.RemoteIpAddress is not { } ip)
        {
            _logger.LogWarning("Endpoint {Name} reached with no remote IP!", epName);
            return BadRequest();
        }

        IActionResult redirectResult = RedirectToPage("/admin/login");
        if (Request.Headers.Referer is var referer && !string.IsNullOrWhiteSpace(referer.ToString()))
        {
            _logger.LogInformation("Endpoint {Name} reached by {Host} with referer {Referer}", epName, ip, referer);
            redirectResult = Redirect(referer!);
        }

        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            _logger.LogInformation("Login attempt from {Host} has empty login", ip);
            return redirectResult;
        }

        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            _logger.LogInformation("Login attempt from {Host} with login {Login} has empty password", ip, emailAddress);
            return redirectResult;
        }

        if (!_userService.VerifyLogin(emailAddress, password, out IUser? user))
        {
            _logger.LogInformation("Login attempt from {Host} with login {Login} failed", ip, emailAddress);
            return redirectResult;
        }

        if (!string.IsNullOrWhiteSpace(user.Totp))
        {
            // mfa required
            _logger.LogInformation("Login attempt from {Host} with login {Login} requires MFA", ip, emailAddress);
            IMfaToken token = _userService.CreateMfaToken(user);
            return RedirectToPage("/admin/multifactorstep", new { token = token.Token });
        }

        ISession session = _sessionService.CreateSession(Request, user);
        _sessionService.SaveSessionCookie(Response, session);
        _logger.LogInformation("Login attempt from {Host} with login {Login} succeeded", ip, emailAddress);
        return redirectResult;
    }

    /// <summary>
    ///     Signs the client out of its current session.
    /// </summary>
    /// <returns>The result of the sign-out process.</returns>
    [HttpGet("signout")]
    public IActionResult DoSignOut()
    {
        string epName = nameof(DoSignOut);
        if (Request.HttpContext.Connection.RemoteIpAddress is not { } ip)
        {
            _logger.LogWarning("Endpoint {Name} reached with no remote IP!", epName);
            return BadRequest();
        }

        IActionResult redirectResult = RedirectToPage("/admin/login");
        if (Request.Headers.Referer is var referer && !string.IsNullOrWhiteSpace(referer.ToString()))
        {
            _logger.LogInformation("Endpoint {Name} reached by {Host} with referer {Referer}", epName, ip, referer);
            redirectResult = Redirect(referer!);
        }

        if (!_sessionService.TryGetSession(HttpContext.Request, out ISession? session))
        {
            _logger.LogInformation("Session sign-out from {Host} requested with no valid session", ip);
            return redirectResult;
        }

        _sessionService.DeleteSession(session);
        _sessionService.DeleteSessionCookie(HttpContext.Response);
        _logger.LogInformation("Session sign-out from {Host} completed successfully", ip);
        return redirectResult;
    }
}
