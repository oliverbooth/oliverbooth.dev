using Markdig;
using Microsoft.Extensions.DependencyInjection;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Data.Web;
using OliverBooth.Common.Markdown.Template;
using OliverBooth.Common.Services;
using X10D.Hosting.DependencyInjection;

namespace OliverBooth.Common.Extensions;

/// <summary>
///     Extension methods for dependency injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    ///     Adds all required services provided by the assembly to the current <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="collection">The <see cref="IServiceCollection" /> to add the service to.</param>
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton(provider => new MarkdownPipelineBuilder()
            // .Use<TimestampExtension>()
            .Use(new TemplateExtension(provider.GetRequiredService<ITemplateService>()))
            .UseAdvancedExtensions()
            .UseBootstrap()
            .UseEmojiAndSmiley()
            .UseSmartyPants()
            .Build());

        collection.AddDbContextFactory<BlogContext>();
        collection.AddDbContextFactory<WebContext>();

        collection.AddSingleton<IBlogPostService, BlogPostService>();
        collection.AddSingleton<IContactService, ContactService>();
        collection.AddSingleton<IProjectService, ProjectService>();
        collection.AddSingleton<IReadingListService, ReadingListService>();
        collection.AddSingleton<ITemplateService, TemplateService>();

        collection.AddHostedSingleton<ISessionService, SessionService>();
        collection.AddHostedSingleton<IUserService, UserService>();
    }
}
