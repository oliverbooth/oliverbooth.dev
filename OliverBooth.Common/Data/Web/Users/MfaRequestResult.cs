using OliverBooth.Common.Services;

namespace OliverBooth.Common.Data.Web.Users;

/// <summary>
///     An enumeration of possible results for <see cref="IUserService.VerifyMfaRequest" />.
/// </summary>
public enum MfaRequestResult
{
    /// <summary>
    ///     The request was successful.
    /// </summary>
    Success,

    /// <summary>
    ///     The wrong code was entered.
    /// </summary>
    InvalidTotp,

    /// <summary>
    ///     The MFA token has expired.
    /// </summary>
    TokenExpired,

    /// <summary>
    ///     Too many attempts were made by the user.
    /// </summary>
    TooManyAttempts,
}
