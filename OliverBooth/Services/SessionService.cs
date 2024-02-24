using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Web;
using ISession = OliverBooth.Data.Blog.ISession;

namespace OliverBooth.Services;

internal sealed class SessionService : ISessionService
{
    private readonly ILogger<SessionService> _logger;
    private readonly IUserService _userService;
    private readonly IDbContextFactory<BlogContext> _blogContextFactory;

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
        _blogContextFactory = blogContextFactory;
    }

    /// <inheritdoc />
    public ISession CreateSession(HttpRequest request, IUser user)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (user is null) throw new ArgumentNullException(nameof(user));

        using BlogContext context = _blogContextFactory.CreateDbContext();
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
        using BlogContext context = _blogContextFactory.CreateDbContext();
        context.Sessions.Remove((Session)session);
        context.SaveChanges();
    }

    /// <inheritdoc />
    public bool TryGetSession(Guid sessionId, [NotNullWhen(true)] out ISession? session)
    {
        using BlogContext context = _blogContextFactory.CreateDbContext();
        session = context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        return session is not null;
    }

    /// <inheritdoc />
    public bool TryGetSession(HttpRequest request, [NotNullWhen(true)] out ISession? session)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        session = null;
        IPAddress? remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null) return false;

        if (!request.Cookies.TryGetValue("sid", out string? sessionIdCookie))
            return false;

        Span<byte> bytes = stackalloc byte[16];
        if (!Convert.TryFromBase64Chars(sessionIdCookie, bytes, out int bytesWritten) || bytesWritten < 16)
            return false;

        var sessionId = new Guid(bytes);
        return TryGetSession(sessionId, out session);
    }

    /// <inheritdoc />
    public bool ValidateSession(HttpRequest request, ISession session)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (session is null) throw new ArgumentNullException(nameof(session));

        IPAddress? remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null) return false;

        if (session.Expires >= DateTimeOffset.UtcNow)
        {
            return false;
        }

        Span<byte> remoteAddressBytes = stackalloc byte[16];
        Span<byte> sessionAddressBytes = stackalloc byte[16];
        if (!remoteIpAddress.TryWriteBytes(remoteAddressBytes, out _) ||
            !session.IpAddress.TryWriteBytes(sessionAddressBytes, out _))
            return false;

        if (!remoteAddressBytes.SequenceEqual(sessionAddressBytes))
            return false;

        if (_userService.TryGetUser(session.UserId, out _))
            return false;

        return true;
    }
}
