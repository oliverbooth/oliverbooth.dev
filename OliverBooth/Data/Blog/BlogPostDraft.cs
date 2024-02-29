using System.ComponentModel.DataAnnotations.Schema;
using SmartFormat;

namespace OliverBooth.Data.Blog;

/// <inheritdoc />
internal sealed class BlogPostDraft : IBlogPostDraft
{
    /// <inheritdoc />
    [NotMapped]
    public IBlogAuthor Author { get; internal set; } = null!;

    /// <inheritdoc />
    public string Body { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool EnableComments { get; internal set; }

    /// <inheritdoc />
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <inheritdoc />
    public bool IsRedirect { get; internal set; }

    /// <inheritdoc />
    public string? Password { get; internal set; }

    /// <inheritdoc />
    public Uri? RedirectUrl { get; internal set; }

    /// <inheritdoc />
    public string Slug { get; internal set; } = string.Empty;

    /// <inheritdoc />
    public IReadOnlyList<string> Tags { get; internal set; } = ArraySegment<string>.Empty;

    /// <inheritdoc />
    public string Title { get; set; } = string.Empty;

    /// <inheritdoc />
    public DateTimeOffset Updated { get; internal set; }

    /// <inheritdoc />
    public BlogPostVisibility Visibility { get; internal set; }

    /// <inheritdoc />
    public int? WordPressId { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the author of this blog post.
    /// </summary>
    /// <value>The ID of the author of this blog post.</value>
    internal Guid AuthorId { get; set; }

    /// <summary>
    ///     Gets or sets the base URL of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus base URL.</value>
    internal string? DisqusDomain { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus identifier.</value>
    internal string? DisqusIdentifier { get; set; }

    /// <summary>
    ///     Gets or sets the URL path of the Disqus comments for the blog post.
    /// </summary>
    /// <value>The Disqus URL path.</value>
    internal string? DisqusPath { get; set; }

    /// <summary>
    ///     Constructs a <see cref="BlogPostDraft" /> by copying values from an existing <see cref="BlogPost" />.
    /// </summary>
    /// <param name="post">The existing <see cref="BlogPost" />.</param>
    /// <returns>The newly-constructed <see cref="BlogPostDraft" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="post" /> is <see langword="null" />.</exception>
    public static BlogPostDraft CreateFromBlogPost(BlogPost post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        return new BlogPostDraft
        {
            AuthorId = post.AuthorId,
            Body = post.Body,
            DisqusDomain = post.DisqusDomain,
            DisqusIdentifier = post.DisqusIdentifier,
            DisqusPath = post.DisqusPath,
            EnableComments = post.EnableComments,
            IsRedirect = post.IsRedirect,
            Password = post.Password,
            RedirectUrl = post.RedirectUrl,
            Tags = post.Tags,
            Title = post.Title,
            Visibility = post.Visibility,
            WordPressId = post.WordPressId
        };
    }

    /// <summary>
    ///     Gets the Disqus domain for the blog post.
    /// </summary>
    /// <returns>The Disqus domain.</returns>
    public string GetDisqusDomain()
    {
        return string.IsNullOrWhiteSpace(DisqusDomain)
            ? "https://oliverbooth.dev/blog"
            : Smart.Format(DisqusDomain, this);
    }

    /// <inheritdoc />
    public string GetDisqusIdentifier()
    {
        return string.IsNullOrWhiteSpace(DisqusIdentifier) ? $"post-{Id}" : Smart.Format(DisqusIdentifier, this);
    }

    /// <inheritdoc />
    public string GetDisqusPostId()
    {
        return WordPressId?.ToString() ?? Id.ToString();
    }
}
