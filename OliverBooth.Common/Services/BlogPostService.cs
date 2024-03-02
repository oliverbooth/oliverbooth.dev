using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Data.Web.Users;

namespace OliverBooth.Common.Services;

/// <summary>
///     Represents an implementation of <see cref="IBlogPostService" />.
/// </summary>
internal sealed class BlogPostService : IBlogPostService
{
    /*private static readonly JsonSerializerOptions EditorJsOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        Converters =
        {
            new ParagraphBlockConverter(),
            new HeadingBlockConverter(),
            new MarkdownDocumentConverter()
        }
    };
    */

    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly IUserService _userService;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="BlogContext" />.
    /// </param>
    /// <param name="userService">The <see cref="IUserService" />.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public BlogPostService(IDbContextFactory<BlogContext> dbContextFactory,
        IUserService userService,
        MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _userService = userService;
        _markdownPipeline = markdownPipeline;
    }

    /// <inheritdoc />
    public string GetBlogPostEditorObject(IBlogPost post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        /*var document = (JsonDocument)Markdig.Markdown.Convert(post.Body, new JsonRenderer(), _markdownPipeline);
        return JsonSerializer.Serialize(document, EditorJsOptions);*/
        return """{"blocks":{}}""";
    }

    /// <inheritdoc />
    public int GetBlogPostCount()
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts.Count();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPostDraft> GetDrafts(IBlogPost post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPostDrafts.Where(d => d.Id == post.Id).OrderBy(d => d.Updated).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPost> GetAllBlogPosts(int limit = -1,
        BlogPostVisibility visibility = BlogPostVisibility.Published)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        IQueryable<BlogPost> ordered = context.BlogPosts;
        if (visibility != (BlogPostVisibility)(-1))
        {
            ordered = ordered.Where(p => p.Visibility == visibility);
        }

        ordered = ordered.OrderByDescending(post => post.Published);
        if (limit > -1)
        {
            ordered = ordered.Take(limit);
        }

        return ordered.AsEnumerable().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPost> GetBlogPosts(int page, int pageSize = 10)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == BlogPostVisibility.Published)
            .OrderByDescending(post => post.Published)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToArray().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public IBlogPost? GetNextPost(IBlogPost blogPost)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == BlogPostVisibility.Published)
            .OrderBy(post => post.Published)
            .FirstOrDefault(post => post.Published > blogPost.Published);
    }

    /// <inheritdoc />
    public IBlogPost? GetPreviousPost(IBlogPost blogPost)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == BlogPostVisibility.Published)
            .OrderByDescending(post => post.Published)
            .FirstOrDefault(post => post.Published < blogPost.Published);
    }

    /// <inheritdoc />
    public string RenderExcerpt(IBlogPost post, out bool wasTrimmed)
    {
        string body = post.Body;
        int moreIndex = body.IndexOf("<!--more-->", StringComparison.Ordinal);

        if (moreIndex == -1)
        {
            string excerpt = body.Truncate(255, "...");
            wasTrimmed = body.Length > 255;
            return Markdig.Markdown.ToHtml(excerpt, _markdownPipeline);
        }

        wasTrimmed = true;
        return Markdig.Markdown.ToHtml(body[..moreIndex], _markdownPipeline);
    }

    /// <inheritdoc />
    public string RenderPost(IBlogPost post)
    {
        return Markdig.Markdown.ToHtml(post.Body, _markdownPipeline);
    }

    /// <inheritdoc />
    public bool TryGetPost(Guid id, [NotNullWhen(true)] out IBlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.Find(id);
        if (post is null)
        {
            return false;
        }

        CacheAuthor((BlogPost)post);
        return true;
    }

    /// <inheritdoc />
    public bool TryGetPost(int id, [NotNullWhen(true)] out IBlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.FirstOrDefault(p => p.WordPressId == id);
        if (post is null)
        {
            return false;
        }

        CacheAuthor((BlogPost)post);
        return true;
    }

    /// <inheritdoc />
    public bool TryGetPost(DateOnly publishDate, string slug, [NotNullWhen(true)] out IBlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.FirstOrDefault(post => post.Published.Year == publishDate.Year &&
                                                        post.Published.Month == publishDate.Month &&
                                                        post.Published.Day == publishDate.Day &&
                                                        post.Slug == slug);

        if (post is null)
        {
            return false;
        }

        CacheAuthor((BlogPost)post);
        return true;
    }

    /// <inheritdoc />
    public void UpdatePost(IBlogPost post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        using BlogContext context = _dbContextFactory.CreateDbContext();
        BlogPost cached = context.BlogPosts.First(p => p.Id == post.Id);
        context.BlogPostDrafts.Add(BlogPostDraft.CreateFromBlogPost(cached));
        context.Update(post);
        context.SaveChanges();
    }

    private BlogPost CacheAuthor(BlogPost post)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (post.Author is not null)
        {
            return post;
        }

        if (_userService.TryGetUser(post.AuthorId, out IUser? user) && user is IAuthor author)
        {
            post.Author = author;
        }

        return post;
    }
}
