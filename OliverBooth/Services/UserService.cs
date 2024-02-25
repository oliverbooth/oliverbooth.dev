using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Web;
using BC = BCrypt.Net.BCrypt;
using Timer = System.Timers.Timer;

namespace OliverBooth.Services;

/// <summary>
///     Represents an implementation of <see cref="IUserService" />.
/// </summary>
internal sealed class UserService : BackgroundService, IUserService
{
    private static readonly RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();
    private readonly IDbContextFactory<WebContext> _dbContextFactory;
    private readonly ConcurrentDictionary<Guid, IUser> _userCache = new();
    private readonly ConcurrentDictionary<string, MfaToken> _tokenCache = new();
    private readonly Timer _tokenClearTimer = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="WebContext" />.
    /// </param>
    public UserService(IDbContextFactory<WebContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;

        _tokenClearTimer.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
        _tokenClearTimer.Elapsed += (_, _) => ClearExpiredTokens();
    }

    /// <inheritdoc />
    public void ClearExpiredTokens()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var keysToRemove = new string[_tokenCache.Count];
        var insertionIndex = 0;

        foreach (var (key, token) in _tokenCache)
        {
            if (token.Expires <= now)
            {
                keysToRemove[insertionIndex++] = key;
            }
        }

        for (var index = 0; index < insertionIndex; index++)
        {
            _tokenCache.TryRemove(keysToRemove[index], out _);
        }
    }

    /// <inheritdoc />
    public void ClearTokens()
    {
        _tokenCache.Clear();
    }

    /// <inheritdoc />
    public IMfaToken CreateMfaToken(IUser user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }


        DateTimeOffset now = DateTimeOffset.UtcNow;
        var token = new MfaToken
        {
            Token = CreateToken(),
            User = user,
            Attempts = 0,
            Created = now,
            Expires = now + TimeSpan.FromMinutes(5)
        };

        _tokenCache[token.Token] = token;
        return token;

        // while we do want a string, BitConvert.ToString requires a heap byte array
        // which is just very not pog. so this method behaves the same but uses a Span<byte>
        // while still returning a string necessary for the IMfaToken model
        static string CreateToken()
        {
            ReadOnlySpan<char> hexChars = "0123456789ABCDEF";
            Span<char> chars = stackalloc char[128];
            Span<byte> buffer = stackalloc byte[64];
            RandomNumberGenerator.GetBytes(buffer);

            for (var index = 0; index < buffer.Length; index++)
            {
                int byteValue = buffer[index];
                chars[index * 2] = hexChars[byteValue >> 4];
                chars[index * 2 + 1] = hexChars[byteValue & 0xF];
            }

            return chars.ToString();
        }
    }

    /// <inheritdoc />
    public void DeleteToken(string token)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        _tokenCache.TryRemove(token, out _);
    }

    /// <inheritdoc />
    public bool TryGetUser(Guid id, [NotNullWhen(true)] out IUser? user)
    {
        if (_userCache.TryGetValue(id, out user))
        {
            return true;
        }

        using WebContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.Find(id);

        if (user is not null)
        {
            _userCache.TryAdd(id, user);
        }

        return user is not null;
    }

    /// <inheritdoc />
    public bool VerifyLogin(string email, string password, [NotNullWhen(true)] out IUser? user)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.FirstOrDefault(u => u.EmailAddress == email);
        return user is not null && BC.Verify(password, ((User)user).Password);
    }

    /// <inheritdoc />
    public MfaRequestResult VerifyMfaRequest(string token, string totp, out IUser? user)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (totp is null)
        {
            throw new ArgumentNullException(nameof(totp));
        }

        user = null;

        if (!_tokenCache.TryGetValue(token, out MfaToken? mfaToken))
        {
            return MfaRequestResult.TokenExpired;
        }

        if (!mfaToken.User.TestTotp(totp))
        {
            mfaToken.Attempts++;
            if (mfaToken.Attempts == 4)
            {
                return MfaRequestResult.TooManyAttempts;
            }

            return MfaRequestResult.InvalidTotp;
        }

        user = mfaToken.User;
        return MfaRequestResult.Success;
    }

    /// <inheritdoc />
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _tokenClearTimer.Stop();
        return base.StopAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ClearTokens();
        _tokenClearTimer.Start();
        return Task.CompletedTask;
    }
}
