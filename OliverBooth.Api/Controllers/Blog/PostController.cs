using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Api.Data;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Services;

namespace OliverBooth.Api.Controllers.Blog;

/// <summary>
///     Represents an API controller which allows reading and writing of blog posts.
/// </summary>
[ApiController]
[Route("v{version:apiVersion}/blog/post")]
[Produces("application/json")]
[ApiVersion(1)]
public sealed class PostController : ControllerBase
{
    private const int ItemsPerPage = 10;
    private readonly IBlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PostController" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="IBlogPostService" />.</param>
    public PostController(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Returns a collection of all blog posts on the specified page.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <returns>An array of <see cref="IBlogPost" /> objects.</returns>
    [HttpGet("all/{page:int?}")]
    [EndpointDescription("Returns a collection of all blog posts on the specified page.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlogPost[]))]
    public IActionResult All(int page = 0)
    {
        IReadOnlyList<IBlogPost> allPosts = _blogPostService.GetBlogPosts(page, ItemsPerPage);
        return Ok(allPosts.Select(post => BlogPost.FromBlogPost(post, _blogPostService)));
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
    ///     Returns an object representing the blog post with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the blog post.</param>
    /// <returns>An object representing the blog post.</returns>
    [HttpGet("{id:guid}")]
    [EndpointDescription("Returns an object representing the blog post with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlogPost))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPost(Guid id)
    {
        if (!_blogPostService.TryGetPost(id, out IBlogPost? post))
        {
            return NotFound();
        }

        return Ok(BlogPost.FromBlogPost(post, _blogPostService, true));
    }

    /// <summary>
    ///     Returns a collection of all blog posts which contain the specified tag on the specified page.
    /// </summary>
    /// <param name="tag">The tag for which to search.</param>
    /// <param name="page">The page number.</param>
    /// <returns>An array of <see cref="IBlogPost" /> objects.</returns>
    [HttpGet("tagged/{tag}/{page:int?}")]
    [EndpointDescription("Returns a collection of all blog posts which contain the specified tag on the specified page.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlogPost[]))]
    public IActionResult Tagged(string tag, int page = 0)
    {
        tag = tag.Replace('-', ' ').ToLowerInvariant();

        IReadOnlyList<IBlogPost> allPosts = _blogPostService.GetBlogPosts(page, ItemsPerPage);
        allPosts = allPosts.Where(post => post.Tags.Contains(tag)).ToList();
        return Ok(allPosts.Select(post => BlogPost.FromBlogPost(post, _blogPostService)));
    }
}
