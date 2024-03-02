namespace OliverBooth.Common.Data.Web.Users;

/// <summary>
///     Represents a temporary token used to correlate MFA attempts with the user.
/// </summary>
public interface IMfaToken
{
    /// <summary>
    ///     Gets a value indicating the number of attempts made with this token. 
    /// </summary>
    /// <value>The number of attempts.</value>
    int Attempts { get; }

    /// <summary>
    ///     Gets the date and time at which this token was created.
    /// </summary>
    /// <value>The creation timestamp.</value>
    DateTimeOffset Created { get; }

    /// <summary>
    ///     Gets the date and time at which this token expires.
    /// </summary>
    /// <value>The expiration timestamp.</value>
    DateTimeOffset Expires { get; }

    /// <summary>
    ///     Gets the 512-bit token for MFA.
    /// </summary>
    /// <value>The temporary MFA token.</value>
    string Token { get; }

    /// <summary>
    ///     Gets the user to whom this token is associated.
    /// </summary>
    /// <value>The user.</value>
    IUser User { get; }
}
