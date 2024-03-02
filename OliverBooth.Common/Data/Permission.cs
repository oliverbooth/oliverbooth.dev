namespace OliverBooth.Common.Data;

/// <summary>
///     Represents a permission.
/// </summary>
public struct Permission
{
    /// <summary>
    ///     Represents a permission that grants all scopes.
    /// </summary>
    public static readonly Permission Administrator = new("*");
    
    /// <summary>
    ///     Initializes a new instance of the <see cref="Permission" /> struct.
    /// </summary>
    /// <param name="name">The name of the permission.</param>
    /// <param name="isAllowed">
    ///     <see langword="true" /> if the permission is allowed; otherwise, <see langword="false" />.
    /// </param>
    public Permission(string name, bool isAllowed = true)
    {
        Name = name;
        IsAllowed = isAllowed;
    }

    /// <summary>
    ///     Gets the name of this permission.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>
    ///     Gets a value indicating whether this permission is allowed.
    /// </summary>
    /// <value><see langword="true" /> if the permission is allowed; otherwise, <see langword="false" />.</value>
    public bool IsAllowed { get; }
}
