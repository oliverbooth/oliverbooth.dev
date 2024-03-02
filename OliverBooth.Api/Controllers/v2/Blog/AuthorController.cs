using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Api.Data;
using OliverBooth.Common.Data.Web.Users;
using OliverBooth.Common.Services;

namespace OliverBooth.Api.Controllers.v2.Blog;

/// <summary>
///     Represents an API controller which allows reading authors of blog posts.
/// </summary>
[ApiController]
[Route("v{version:apiVersion}/blog/author")]
[Produces("application/json")]
[ApiVersion(2)]
public sealed class AuthorController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorController" /> class.
    /// </summary>
    /// <param name="userService">The <see cref="IUserService" />.</param>
    public AuthorController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    ///     Returns an object representing the author with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the author.</param>
    /// <returns>An object representing the author.</returns>
    [HttpGet("{id:guid}")]
    [EndpointDescription("Returns an object representing the author with the specified ID.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Author))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAuthor(Guid id)
    {
        if (!_userService.TryGetUser(id, out IUser? author))
        {
            return NotFound();
        }

        return Ok(Author.FromUser(author));
    }
}
