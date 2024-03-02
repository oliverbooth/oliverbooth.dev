using OliverBooth.Common.Data.Web.Contact;

namespace OliverBooth.Common.Services;

/// <summary>
///     Represents a service for managing contact information.
/// </summary>
public interface IContactService
{
    /// <summary>
    ///     Gets the blacklist.
    /// </summary>
    IReadOnlyCollection<IBlacklistEntry> GetBlacklist();
}
