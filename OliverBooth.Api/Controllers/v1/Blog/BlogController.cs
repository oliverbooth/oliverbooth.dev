using Asp.Versioning;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Api.Data;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Data.Web.Users;
using OliverBooth.Common.Services;

namespace OliverBooth.Api.Controllers.v1.Blog;

[ApiController]
[Route("blog")]
[Produces("application/json")]
[ApiVersion(1)]
[Obsolete("API v1 is deprecated and will be removed in future. Use /v2")]
public sealed class BlogController : ControllerBase
{
    private readonly IBlogPostService _blogPostService;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogController" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="IBlogPostService" />.</param>
    /// <param name="userService">The <see cref="IUserService" />.</param>
    public BlogController(IBlogPostService blogPostService, IUserService userService)
    {
        _blogPostService = blogPostService;
        _userService = userService;
    }

    /// <summary>
    ///     Returns the number of publicly published blog posts.
    /// </summary>
    /// <returns>The number of publicly published blog posts.</returns>
    [HttpGet("count")]
    [EndpointDescription("Returns the number of publicly published blog posts.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Count()
    {
        return Ok(new { count = _blogPostService.GetBlogPostCount() });
    }

    /// <summary>
    ///     Returns a collection of all blog posts on the specified page.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <returns>An array of <see cref="IBlogPost" /> objects.</returns>
    [HttpGet("posts/{page:int?}")]
    [EndpointDescription("Returns a collection of all blog posts on the specified page.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlogPost[]))]
    public IActionResult GetAllBlogPosts(int page = 0)
    {
        const int itemsPerPage = 10;
        IReadOnlyList<IBlogPost> allPosts = _blogPostService.GetBlogPosts(page, itemsPerPage);
        return Ok(allPosts.Select(post => CreatePostObject(post)));
    }

    /// <summary>
    ///     Returns a collection of all blog posts which contain the specified tag on the specified page.
    /// </summary>
    /// <param name="tag">The tag for which to search.</param>
    /// <param name="page">The page number.</param>
    /// <returns>An array of <see cref="IBlogPost" /> objects.</returns>
    [HttpGet("posts/tagged/{tag}/{page:int?}")]
    [EndpointDescription("Returns a collection of all blog posts which contain the specified tag on the specified page.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlogPost[]))]
    public IActionResult GetTaggedBlogPosts(string tag, int page = 0)
    {
        const int itemsPerPage = 10;
        tag = tag.Replace('-', ' ').ToLowerInvariant();

        IReadOnlyList<IBlogPost> allPosts = _blogPostService.GetBlogPosts(page, itemsPerPage);
        allPosts = allPosts.Where(post => post.Tags.Contains(tag)).ToList();
        return Ok(allPosts.Select(post => CreatePostObject(post)));
    }

    /// <summary>
    ///     Returns an object representing the author with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the author.</param>
    /// <returns>An object representing the author.</returns>
    [HttpGet("author/{id:guid}")]
    [EndpointDescription("Returns an object representing the author with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Author))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAuthor(Guid id)
    {
        if (!_userService.TryGetUser(id, out IUser? author)) return NotFound();

        return Ok(new
        {
            id = author.Id,
            name = author.DisplayName,
            avatarUrl = author.AvatarUrl,
        });
    }

    /// <summary>
    ///     Returns an object representing the blog post with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the blog post.</param>
    /// <returns>An object representing the blog post.</returns>
    [HttpGet("post/{id:guid?}")]
    [EndpointDescription("Returns an object representing the blog post with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlogPost))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPost(Guid id)
    {
        if (!_blogPostService.TryGetPost(id, out IBlogPost? post)) return NotFound();
        return Ok(CreatePostObject(post, true));
    }

    private object CreatePostObject(IBlogPost post, bool includeContent = false)
    {
        return new
        {
            id = post.Id,
            commentsEnabled = post.EnableComments,
            identifier = post.GetDisqusIdentifier(),
            author = post.Author.Id,
            title = post.Title,
            published = post.Published.ToUnixTimeSeconds(),
            updated = post.Updated?.ToUnixTimeSeconds(),
            formattedPublishDate = post.Published.ToString("dddd, d MMMM yyyy HH:mm"),
            formattedUpdateDate = post.Updated?.ToString("dddd, d MMMM yyyy HH:mm"),
            humanizedTimestamp = post.Updated?.Humanize() ?? post.Published.Humanize(),
            excerpt = _blogPostService.RenderExcerpt(post, out bool trimmed),
            content = includeContent ? _blogPostService.RenderPost(post) : null,
            trimmed,
            tags = post.Tags.Select(t => t.Replace(' ', '-')),
            url = new
            {
                year = post.Published.ToString("yyyy"),
                month = post.Published.ToString("MM"),
                day = post.Published.ToString("dd"),
                slug = post.Slug
            }
        };
    }
}
