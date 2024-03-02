using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Data.Web.Users;
using OliverBooth.Common.Services;

namespace OliverBooth.Pages.Admin;

public class EditBlogPost : PageModel
{
    private readonly IBlogPostService _blogPostService;
    private readonly ISessionService _sessionService;

    public EditBlogPost(IBlogPostService blogPostService, ISessionService sessionService)
    {
        _blogPostService = blogPostService;
        _sessionService = sessionService;
    }

    public IUser CurrentUser { get; private set; } = null!;

    public IBlogPost BlogPost { get; private set; } = null!;

    public IActionResult OnGet([FromRoute(Name = "id")] Guid postId)
    {
        if (!_sessionService.TryGetCurrentUser(Request, Response, out IUser? user))
        {
            return RedirectToPage("/admin/login");
        }

        if (_blogPostService.TryGetPost(postId, out IBlogPost? post))
        {
            BlogPost = post;
        }

        CurrentUser = user;
        return Page();
    }
}
