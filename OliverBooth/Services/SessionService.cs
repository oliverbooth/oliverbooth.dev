using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Web;
using ISession = OliverBooth.Data.Web.ISession;

namespace OliverBooth.Services;

internal sealed class SessionService : BackgroundService, ISessionService
{
    private readonly ILogger<SessionService> _logger;
    private readonly IUserService _userService;
    private readonly IDbContextFactory<WebContext> _webContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="blogContextFactory">The <see cref="BlogContext" /> factory.</param>
    /// <param name="webContextFactory">The <see cref="WebContext" /> factory.</param>
    public SessionService(ILogger<SessionService> logger,
        IUserService userService,
        IDbContextFactory<BlogContext> blogContextFactory,
        IDbContextFactory<WebContext> webContextFactory)
    {
        _logger = logger;
        _userService = userService;
        _webContextFactory = webContextFactory;
    }

    /// <inheritdoc />
    public ISession CreateSession(HttpRequest request, IUser user)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        using WebContext context = _webContextFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;
        var session = new Session
        {
            UserId = user.Id,
            IpAddress = request.HttpContext.Connection.RemoteIpAddress!,
            Created = now,
            Updated = now,
            LastAccessed = now,
            Expires = now + TimeSpan.FromDays(1),
            RequiresTotp = !string.IsNullOrWhiteSpace(user.Totp)
        };
        EntityEntry<Session> entry = context.Sessions.Add(session);
        context.SaveChanges();
        return entry.Entity;
    }

    /// <inheritdoc />
    public void DeleteSession(ISession session)
    {
        using WebContext context = _webContextFactory.CreateDbContext();
        context.Sessions.Remove((Session)session);
        context.SaveChanges();
    }

    /// <inheritdoc />
    public IActionResult DeleteSessionCookie(HttpResponse response)
    {
        response.Cookies.Delete("sid");
        return new RedirectToPageResult("/Admin/Login");
    }

    /// <inheritdoc />
    public void SaveSessionCookie(HttpResponse response, ISession session)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        Span<byte> buffer = stackalloc byte[16];
        if (!session.Id.TryWriteBytes(buffer))
        {
            return;
        }

        IPAddress? remoteIpAddress = response.HttpContext.Connection.RemoteIpAddress;
        _logger.LogDebug("Writing cookie 'sid' to HTTP response for {RemoteAddr}", remoteIpAddress);
        response.Cookies.Append("sid", Convert.ToBase64String(buffer), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow + TimeSpan.FromDays(30),
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
    }

    /// <inheritdoc />
    public bool TryGetCurrentUser(HttpRequest request, HttpResponse response, [NotNullWhen(true)] out IUser? user)
    {
        user = null;
        
        if (!TryGetSession(request, out ISession? session))
        {
            _logger.LogDebug("Session not found; redirecting");
            DeleteSessionCookie(response);
            return false;
        }

        if (!ValidateSession(request, session))
        {
            _logger.LogDebug("Session invalid; redirecting");
            DeleteSessionCookie(response);
            return false;
        }

        if (!_userService.TryGetUser(session.UserId, out user))
        {
            _logger.LogDebug("User not found; redirecting");
            DeleteSessionCookie(response);
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool TryGetSession(Guid sessionId, [NotNullWhen(true)] out ISession? session)
    {
        using WebContext context = _webContextFactory.CreateDbContext();
        session = context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        return session is not null;
    }

    /// <inheritdoc />
    public bool TryGetSession(HttpRequest request, [NotNullWhen(true)] out ISession? session)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        session = null;
        IPAddress? remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null)
        {
            return false;
        }

        if (!request.Cookies.TryGetValue("sid", out string? sessionIdCookie))
        {
            return false;
        }

        Span<byte> bytes = stackalloc byte[16];
        if (!Convert.TryFromBase64Chars(sessionIdCookie, bytes, out int bytesWritten) || bytesWritten < 16)
        {
            return false;
        }

        var sessionId = new Guid(bytes);
        return TryGetSession(sessionId, out session);
    }

    /// <inheritdoc />
    public bool ValidateSession(HttpRequest request, ISession session)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        IPAddress? remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null)
        {
            return false;
        }

        if (session.Expires <= DateTimeOffset.UtcNow)
        {
            _logger.LogInformation("Session {Id} has expired (client {Ip})", session.Id, remoteIpAddress);
            return false;
        }

        Span<byte> remoteAddressBytes = stackalloc byte[16];
        Span<byte> sessionAddressBytes = stackalloc byte[16];
        if (!remoteIpAddress.TryWriteBytes(remoteAddressBytes, out _) ||
            !session.IpAddress.TryWriteBytes(sessionAddressBytes, out _))
        {
            return false;
        }

        if (!remoteAddressBytes.SequenceEqual(sessionAddressBytes))
        {
            return false;
        }

        if (!_userService.TryGetUser(session.UserId, out _))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using WebContext context = await _webContextFactory.CreateDbContextAsync(stoppingToken);
        context.Sessions.RemoveRange(context.Sessions.Where(s => s.Expires <= DateTimeOffset.UtcNow));
        await context.SaveChangesAsync(stoppingToken);
    }
}
