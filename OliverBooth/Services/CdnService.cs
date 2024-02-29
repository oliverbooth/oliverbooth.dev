using System.ComponentModel;
using FluentFTP;
using OliverBooth.Data;

namespace OliverBooth.Services;

internal sealed class CdnService : ICdnService
{
    private readonly ILogger<CdnService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAsyncFtpClient _ftpClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CdnService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="ftpClient">The FTP client.</param>
    public CdnService(ILogger<CdnService> logger, IConfiguration configuration, IAsyncFtpClient ftpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _ftpClient = ftpClient;
    }

    /// <inheritdoc />
    public Task<Uri> CreateAssetAsync(FileStream stream, CdnAssetType assetType)
    {
        string filename = Path.GetFileName(stream.Name);
        return CreateAssetAsync(filename, stream, assetType);
    }

    /// <inheritdoc />
    public async Task<Uri> CreateAssetAsync(string filename, Stream stream, CdnAssetType assetType)
    {
        if (filename is null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Filename cannot be empty");
        }

        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new ArgumentException("The provided stream cannot be read.");
        }

        if (!Enum.IsDefined(assetType))
        {
            throw new InvalidEnumArgumentException(nameof(assetType), (int)assetType, typeof(CdnAssetType));
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        string basePath = _configuration["Cdn:Ftp:ChRoot"]!;
        string? relativePath = _configuration[$"Cdn:AssetTypeMap:{assetType:G}"];
        string remotePath = $"{basePath}{relativePath}/{now:yyyy\\/MM}/{filename}";
        _logger.LogDebug("Base path is {Path}", basePath);
        _logger.LogDebug("Relative path is {Path}", relativePath);
        _logger.LogDebug("Full remote path is {Path}", remotePath);

        _logger.LogInformation("Connecting to FTP server");
        await _ftpClient.AutoConnect();

        _logger.LogInformation("Asset will be at {RemotePath}", remotePath);
        await _ftpClient.UploadStream(stream, remotePath, FtpRemoteExists.Skip, true);

        _logger.LogInformation("Asset upload complete. Disconnecting");
        await _ftpClient.Disconnect();

        return GetUri(now, filename, assetType);
    }

    /// <inheritdoc />
    public Uri GetUri(DateOnly date, string filename, CdnAssetType assetType)
    {
        if (filename is null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Filename cannot be empty");
        }

        if (!Enum.IsDefined(assetType))
        {
            throw new InvalidEnumArgumentException(nameof(assetType), (int)assetType, typeof(CdnAssetType));
        }

        string? relativePath = _configuration[$"Cdn:AssetTypeMap:{assetType:G}"];
        string url = $"{_configuration["Cdn:BaseUrl"]}{relativePath}/{date:yyyy\\/MM)}/{filename}";
        return new Uri(url);
    }

    /// <inheritdoc />
    public Uri GetUri(DateTimeOffset date, string filename, CdnAssetType assetType)
    {
        return GetUri(DateOnly.FromDateTime(date.DateTime), filename, assetType);
    }
}
