using System.Net;

namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a login session.
/// </summary>
public interface ISession
{
    /// <summary>
    ///     Gets the date and time at which this session was created.
    /// </summary>
    /// <value>The creation timestamp.</value>
    DateTimeOffset Created { get; }

    /// <summary>
    ///     Gets the date and time at which this session expires.
    /// </summary>
    /// <value>The expiration timestamp.</value>
    DateTimeOffset Expires { get; }

    /// <summary>
    ///     Gets the ID of the session.
    /// </summary>
    /// <value>The ID of the session.</value>
    Guid Id { get; }

    /// <summary>
    ///     Gets the IP address of the session.
    /// </summary>
    /// <value>The IP address.</value>
    IPAddress IpAddress { get; }

    /// <summary>
    ///     Gets the date and time at which this session was last accessed.
    /// </summary>
    /// <value>The last access timestamp.</value>
    DateTimeOffset LastAccessed { get; }

    /// <summary>
    ///     Gets a value indicating whether this session is valid.
    /// </summary>
    /// <value><see langword="true" /> if the session is valid; otherwise, <see langword="false" />.</value>
    bool RequiresTotp { get; }

    /// <summary>
    ///     Gets the date and time at which this session was updated.
    /// </summary>
    /// <value>The update timestamp.</value>
    DateTimeOffset Updated { get; }

    /// <summary>
    ///     Gets the user agent string associated with this session.
    /// </summary>
    /// <value>The user agent string.</value>
    string UserAgent { get; }

    /// <summary>
    ///     Gets the user ID associated with the session.
    /// </summary>
    /// <value>The user ID.</value>
    Guid UserId { get; }
}
