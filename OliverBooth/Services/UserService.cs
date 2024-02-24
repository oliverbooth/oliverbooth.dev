using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Web;
using BC = BCrypt.Net.BCrypt;

namespace OliverBooth.Services;

/// <summary>
///     Represents an implementation of <see cref="IUserService" />.
/// </summary>
internal sealed class UserService : IUserService
{
    private readonly IDbContextFactory<WebContext> _dbContextFactory;
    private readonly ConcurrentDictionary<Guid, IUser> _userCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="WebContext" />.
    /// </param>
    public UserService(IDbContextFactory<WebContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public bool TryGetUser(Guid id, [NotNullWhen(true)] out IUser? user)
    {
        if (_userCache.TryGetValue(id, out user)) return true;

        using WebContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.Find(id);

        if (user is not null) _userCache.TryAdd(id, user);
        return user is not null;
    }

    /// <inheritdoc />
    public bool VerifyLogin(string email, string password, [NotNullWhen(true)] out IUser? user)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.FirstOrDefault(u => u.EmailAddress == email);
        return user is not null && BC.Verify(password, ((User)user).Password);
    }
}
