using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OliverBooth.Common.Data.Blog.Configuration;

internal sealed class BlogPostDraftConfiguration : IEntityTypeConfiguration<BlogPostDraft>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BlogPostDraft> builder)
    {
        builder.ToTable("BlogPostDraft");
        builder.HasKey(e => new { e.Id, e.Updated });

        builder.Property(e => e.Id);
        builder.Property(e => e.Updated).IsRequired();
        builder.Property(e => e.WordPressId).IsRequired(false);
        builder.Property(e => e.Slug).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AuthorId).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Body).IsRequired();
        builder.Property(e => e.IsRedirect).IsRequired();
        builder.Property(e => e.RedirectUrl).HasConversion<UriToStringConverter>().HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.EnableComments).IsRequired();
        builder.Property(e => e.DisqusDomain).IsRequired(false);
        builder.Property(e => e.DisqusIdentifier).IsRequired(false);
        builder.Property(e => e.DisqusPath).IsRequired(false);
        builder.Property(e => e.Visibility).HasConversion(new EnumToStringConverter<BlogPostVisibility>()).IsRequired();
        builder.Property(e => e.Password).HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.Tags).IsRequired()
            .HasConversion(
                tags => string.Join(' ', tags.Select(t => t.Replace(' ', '-'))),
                tags => tags.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Replace('-', ' ')).ToArray());
    }
}
