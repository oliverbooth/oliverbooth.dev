using System.Diagnostics.CodeAnalysis;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service for managing users.
/// </summary>
public interface IUserService
{
    /// <summary>
    ///     Attempts to find a user with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the user to find.</param>
    /// <param name="user">
    ///     When this method returns, contains the user with the specified ID, if the user is found; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a user with the specified ID is found; otherwise, <see langword="false" />.
    /// </returns>
    bool TryGetUser(Guid id, [NotNullWhen(true)] out IUser? user);

    /// <summary>
    ///     Verifies the login information of the specified user.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="password">The password.</param>
    /// <param name="user">
    ///     When this method returns, contains the user associated with the login credentials, or
    ///     <see langword="null" /> if the credentials are invalid.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the login credentials are valid; otherwise, <see langword="false" />.
    /// </returns>
    public bool VerifyLogin(string email, string password, [NotNullWhen(true)] out IUser? user);
}
