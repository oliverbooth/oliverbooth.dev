using System.Net;

namespace OliverBooth.Data.Web;

internal sealed class Session : ISession
{
    /// <inheritdoc />
    public DateTimeOffset Created { get; set; }

    /// <inheritdoc />
    public DateTimeOffset Expires { get; set; }

    /// <inheritdoc />
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <inheritdoc />
    public IPAddress IpAddress { get; set; } = IPAddress.None;

    /// <inheritdoc />
    public DateTimeOffset LastAccessed { get; set; }

    /// <inheritdoc />
    public bool RequiresTotp { get; set; }

    /// <inheritdoc />
    public DateTimeOffset Updated { get; set; }

    /// <inheritdoc />
    public Guid UserId { get; set; }
}
