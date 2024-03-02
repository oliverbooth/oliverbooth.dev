using System.Text.Json.Serialization;
using Humanizer;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Services;

namespace OliverBooth.Api.Data;

internal sealed class BlogPost
{
    [JsonPropertyName("author"), JsonInclude, JsonPropertyOrder(3)]
    public Guid Author { get; private set; }

    [JsonPropertyName("commentsEnabled"), JsonInclude, JsonPropertyOrder(1)]
    public bool CommentsEnabled { get; private set; }

    [JsonPropertyName("content"), JsonInclude, JsonPropertyOrder(11)]
    public string? Content { get; private set; }

    [JsonPropertyName("excerpt"), JsonInclude, JsonPropertyOrder(10)]
    public string Excerpt { get; private set; } = string.Empty;

    [JsonPropertyName("formattedPublishDate"), JsonInclude, JsonPropertyOrder(7)]
    public string FormattedPublishDate { get; private set; } = string.Empty;

    [JsonPropertyName("formattedUpdateDate"), JsonInclude, JsonPropertyOrder(8)]
    public string? FormattedUpdateDate { get; private set; }

    [JsonPropertyName("humanizedTimestamp"), JsonInclude, JsonPropertyOrder(9)]
    public string HumanizedTimestamp { get; private set; } = string.Empty;

    [JsonPropertyName("id"), JsonInclude, JsonPropertyOrder(0)]
    public Guid Id { get; private set; }

    [JsonPropertyName("identifier"), JsonInclude, JsonPropertyOrder(2)]
    public string Identifier { get; private set; } = string.Empty;

    [JsonPropertyName("trimmed"), JsonInclude, JsonPropertyOrder(12)]
    public bool IsTrimmed { get; private set; }

    [JsonPropertyName("published"), JsonInclude, JsonPropertyOrder(5)]
    public long Published { get; private set; }

    [JsonPropertyName("tags"), JsonInclude, JsonPropertyOrder(13)]
    public IEnumerable<string> Tags { get; private set; } = ArraySegment<string>.Empty;

    [JsonPropertyName("title"), JsonInclude, JsonPropertyOrder(4)]
    public string Title { get; private set; } = string.Empty;

    [JsonPropertyName("updated"), JsonInclude, JsonPropertyOrder(6)]
    public long? Updated { get; private set; }

    [JsonPropertyName("url"), JsonInclude, JsonPropertyOrder(14)]
    public object Url { get; private set; } = null!;

    public static BlogPost FromBlogPost(IBlogPost post, IBlogPostService blogPostService,
        bool includeContent = false)
    {
        return new()
        {
            Id = post.Id,
            CommentsEnabled = post.EnableComments,
            Identifier = post.GetDisqusIdentifier(),
            Author = post.Author.Id,
            Title = post.Title,
            Published = post.Published.ToUnixTimeSeconds(),
            Updated = post.Updated?.ToUnixTimeSeconds(),
            FormattedPublishDate = post.Published.ToString("dddd, d MMMM yyyy HH:mm"),
            FormattedUpdateDate = post.Updated?.ToString("dddd, d MMMM yyyy HH:mm"),
            HumanizedTimestamp = post.Updated?.Humanize() ?? post.Published.Humanize(),
            Excerpt = blogPostService.RenderExcerpt(post, out bool trimmed),
            Content = includeContent ? blogPostService.RenderPost(post) : null,
            IsTrimmed = trimmed,
            Tags = post.Tags.Select(t => t.Replace(' ', '-')),
            Url = new
            {
                year = post.Published.ToString("yyyy"),
                month = post.Published.ToString("MM"),
                day = post.Published.ToString("dd"),
                slug = post.Slug
            }
        };
    }
}
