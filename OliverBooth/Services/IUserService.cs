using System.Diagnostics.CodeAnalysis;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service for managing users.
/// </summary>
public interface IUserService
{
    /// <summary>
    ///     Clears all expired tokens.
    /// </summary>
    void ClearExpiredTokens();

    /// <summary>
    ///     Clears all tokens.
    /// </summary>
    void ClearTokens();

    /// <summary>
    ///     Creates a temporary MFA token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to create the token.</param>
    /// <returns>The newly-created token.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="user" /> is <see langword="null" />.</exception>
    IMfaToken CreateMfaToken(IUser user);

    /// <summary>
    ///     Deletes the specified token.
    /// </summary>
    /// <param name="token">The token to delete.</param>
    /// <exception cref="ArgumentNullException"><paramref name="token" /> is <see langword="null" />.</exception>
    void DeleteToken(string token);

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
    bool VerifyLogin(string email, string password, [NotNullWhen(true)] out IUser? user);

    /// <summary>
    ///     Verifies the MFA request for the specified user.
    /// </summary>
    /// <param name="token">The MFA token.</param>
    /// <param name="totp">The user-provided TOTP.</param>
    /// <param name="user">
    ///     When this method returns, contains the user associated with the specified token, if the verification was
    ///     successful; otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    ///     An <see cref="MfaRequestResult" /> representing the result of the request.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="token" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="totp" /> is <see langword="null" />.</para>
    /// </exception>
    MfaRequestResult VerifyMfaRequest(string token, string totp, out IUser? user);
}
