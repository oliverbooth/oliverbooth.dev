using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Web;
using OliverBooth.Services;

namespace OliverBooth.Controllers.Api.v1;

[ApiController]
[Route("api/v{version:apiVersion}/post")]
[ApiVersion(1)]
[Produces("application/json")]
public sealed class BlogPostController : ControllerBase
{
    private readonly ILogger<BlogPostController> _logger;
    private readonly ISessionService _sessionService;
    private readonly IBlogPostService _blogPostService;

    public BlogPostController(ILogger<BlogPostController> logger,
        ISessionService sessionService,
        IBlogPostService blogPostService)
    {
        _logger = logger;
        _sessionService = sessionService;
        _blogPostService = blogPostService;
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> OnPatch([FromRoute] Guid id)
    {
        if (!_sessionService.TryGetCurrentUser(Request, Response, out IUser? user))
        {
            Response.StatusCode = 401;
            return new JsonResult(new { status = 401, message = "Unauthorized" });
        }

        if (!_blogPostService.TryGetPost(id, out IBlogPost? post))
        {
            Response.StatusCode = 404;
            return new JsonResult(new { status = 404, message = "Not Found" });
        }

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        string content = await reader.ReadToEndAsync();

        post.Body = content;
        _blogPostService.UpdatePost(post);

        return new JsonResult(new { status = 200, message = "OK" });
    }
}
