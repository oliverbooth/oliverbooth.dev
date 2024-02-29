using System.ComponentModel;
using OliverBooth.Data;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service which communicates with the CDN server.
/// </summary>
public interface ICdnService
{
    /// <summary>
    ///     Asynchronously uploads a new asset to the CDN server.
    /// </summary>
    /// <param name="stream">A stream containing the data to upload.</param>
    /// <param name="assetType">The type of the asset.</param>
    /// <returns>The URI of the newly-created asset.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">The <paramref name="stream" /> is not readable.</exception>
    /// <exception cref="InvalidEnumArgumentException">
    ///     <paramref name="assetType" /> is not a valid <see cref="CdnAssetType" />.
    /// </exception>
    Task<Uri> CreateAssetAsync(FileStream stream, CdnAssetType assetType);

    /// <summary>
    ///     Asynchronously uploads a new asset to the CDN server.
    /// </summary>
    /// <param name="filename">The filename of the asset.</param>
    /// <param name="stream">A stream containing the data to upload.</param>
    /// <param name="assetType">The type of the asset.</param>
    /// <returns>The URI of the newly-created asset.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="filename" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="stream" /> is <see langword="null" />.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <para><paramref name="filename" /> is empty, or contains only whitespace.</para>
    ///     -or-
    ///     <para>The <paramref name="stream" /> is not readable.</para>
    /// </exception>
    /// <exception cref="InvalidEnumArgumentException">
    ///     <paramref name="assetType" /> is not a valid <see cref="CdnAssetType" />.
    /// </exception>
    Task<Uri> CreateAssetAsync(string filename, Stream stream, CdnAssetType assetType);

    /// <summary>
    ///     Gets the resolved asset URI for the specified date and filename.
    /// </summary>
    /// <param name="date">The date of the asset.</param>
    /// <param name="filename">The filename of the asset.</param>
    /// <param name="assetType">The type of the asset.</param>
    /// <returns>The resolved asset URI.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filename" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="filename" /> is empty, or contains only whitespace.
    /// </exception>
    /// <exception cref="InvalidEnumArgumentException">
    ///     <paramref name="assetType" /> is not a valid <see cref="CdnAssetType" />.
    /// </exception>
    Uri GetUri(DateOnly date, string filename, CdnAssetType assetType);

    /// <summary>
    ///     Gets the resolved asset URI for the specified date and filename.
    /// </summary>
    /// <param name="date">The date of the asset.</param>
    /// <param name="filename">The filename of the asset.</param>
    /// <param name="assetType">The type of the asset.</param>
    /// <returns>The resolved asset URI.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filename" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="filename" /> is empty, or contains only whitespace.
    /// </exception>
    /// <exception cref="InvalidEnumArgumentException">
    ///     <paramref name="assetType" /> is not a valid <see cref="CdnAssetType" />.
    /// </exception>
    Uri GetUri(DateTimeOffset date, string filename, CdnAssetType assetType);
}