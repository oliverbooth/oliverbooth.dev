using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Blog;
using BC = BCrypt.Net.BCrypt;

namespace OliverBooth.Services;

/// <summary>
///     Represents an implementation of <see cref="IBlogUserService" />.
/// </summary>
internal sealed class BlogUserService : IBlogUserService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly ConcurrentDictionary<Guid, IUser> _userCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogUserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="BlogContext" />.
    /// </param>
    public BlogUserService(IDbContextFactory<BlogContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public bool TryGetUser(Guid id, [NotNullWhen(true)] out IUser? user)
    {
        if (_userCache.TryGetValue(id, out user)) return true;

        using BlogContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.Find(id);

        if (user is not null) _userCache.TryAdd(id, user);
        return user is not null;
    }

    /// <inheritdoc />
    public bool VerifyLogin(string email, string password, [NotNullWhen(true)] out IUser? user)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.FirstOrDefault(u => u.EmailAddress == email);
        return user is not null && BC.Verify(password, ((User)user).Password);
    }
}
