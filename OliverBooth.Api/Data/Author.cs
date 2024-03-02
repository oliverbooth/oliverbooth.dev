using System.Text.Json.Serialization;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Data.Web.Users;

namespace OliverBooth.Api.Data;

internal sealed class Author
{
    [JsonPropertyName("avatarUrl"), JsonInclude, JsonPropertyOrder(2)]
    public Uri AvatarUrl { get; private set; } = null!;

    [JsonPropertyName("id"), JsonInclude, JsonPropertyOrder(0)]
    public Guid Id { get; private set; }

    [JsonPropertyName("name"), JsonInclude, JsonPropertyOrder(1)]
    public string Name { get; private set; } = string.Empty;

    public static Author FromUser(IUser author)
    {
        return new Author
        {
            Id = author.Id,
            Name = author.DisplayName,
            AvatarUrl = author.AvatarUrl
        };
    }
}
