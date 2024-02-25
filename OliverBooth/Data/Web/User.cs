using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Text;
using OliverBooth.Data.Blog;
using OtpNet;

namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a user.
/// </summary>
internal sealed class User : IUser, IBlogAuthor
{
    /// <inheritdoc cref="IUser.AvatarUrl" />
    [NotMapped]
    public Uri AvatarUrl => GetAvatarUrl();

    /// <inheritdoc />
    public string EmailAddress { get; set; } = string.Empty;

    /// <inheritdoc cref="IUser.DisplayName" />
    public string DisplayName { get; set; } = string.Empty;

    /// <inheritdoc cref="IUser.Id" />
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <inheritdoc />
    public IReadOnlyList<Permission> Permissions { get; private set; } = ArraySegment<Permission>.Empty;

    /// <inheritdoc />
    public DateTimeOffset Registered { get; private set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string? Totp { get; private set; }

    /// <summary>
    ///     Gets or sets the password hash.
    /// </summary>
    /// <value>The password hash.</value>
    internal string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the salt used to hash the password.
    /// </summary>
    /// <value>The salt used to hash the password.</value>
    internal string Salt { get; set; } = string.Empty;

    /// <inheritdoc cref="IUser.GetAvatarUrl" />
    public Uri GetAvatarUrl(int size = 28)
    {
        if (string.IsNullOrWhiteSpace(EmailAddress))
        {
            return new Uri($"https://www.gravatar.com/avatar/0?size={size}");
        }

        ReadOnlySpan<char> span = EmailAddress.AsSpan();
        int byteCount = Encoding.UTF8.GetByteCount(span);
        Span<byte> bytes = stackalloc byte[byteCount];
        Encoding.UTF8.GetBytes(span, bytes);

        Span<byte> hash = stackalloc byte[16];
        MD5.TryHashData(bytes, hash, out _);

        using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
        Span<char> hex = stackalloc char[2];
        for (var index = 0; index < hash.Length; index++)
        {
            if (hash[index].TryFormat(hex, out _, "x2"))
            {
                builder.Append(hex);
            }
            else
            {
                builder.Append("00");
            }
        }

        return new Uri($"https://www.gravatar.com/avatar/{builder}?size={size}");
    }

    /// <inheritdoc />
    public bool HasPermission(Permission permission)
    {
        return HasPermission(permission.Name);
    }

    /// <inheritdoc />
    public bool HasPermission(string permission)
    {
        return (Permissions.Any(p => p.IsAllowed && p.Name == permission) ||
                Permissions.Any(p => p is { IsAllowed: true, Name: "*" })) &&
               !Permissions.Any(p => !p.IsAllowed && p.Name == permission);
    }

    /// <inheritdoc />
    public bool TestCredentials(string password)
    {
        return false;
    }

    /// <inheritdoc />
    public bool TestTotp(string value)
    {
        byte[]? key = Base32Encoding.ToBytes(Totp);
        var totp = new Totp(key);
        return totp.VerifyTotp(value, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}
