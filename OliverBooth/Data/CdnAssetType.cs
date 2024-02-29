namespace OliverBooth.Data;

/// <summary>
///     An enumeration of CDN asset types.
/// </summary>
public enum CdnAssetType
{
    /// <summary>
    ///     An image on a blog post.
    /// </summary>
    BlogImage,

    /// <summary>
    ///     A multimedia asset (audio and video) on a blog post.
    /// </summary>
    BlogMedia,

    /// <summary>
    ///     A raw (typically binary) asset on a blog post.
    /// </summary>
    BlogRaw
}
