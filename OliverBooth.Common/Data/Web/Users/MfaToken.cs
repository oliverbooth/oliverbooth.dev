namespace OliverBooth.Common.Data.Web.Users;

internal sealed class MfaToken : IMfaToken
{
    /// <inheritdoc />
    public int Attempts { get; set; }

    /// <inheritdoc />
    public DateTimeOffset Created { get; set; }

    /// <inheritdoc />
    public DateTimeOffset Expires { get; set; }

    /// <inheritdoc />
    public string Token { get; set; } = string.Empty;

    /// <inheritdoc />
    public IUser User { get; set; } = null!;
}
