using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OliverBooth.Common.Data.Web.Projects;

namespace OliverBooth.Common.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="ProgrammingLanguage" /> entity.
/// </summary>
internal sealed class ProgrammingLanguageConfiguration : IEntityTypeConfiguration<ProgrammingLanguage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProgrammingLanguage> builder)
    {
        builder.ToTable("ProgrammingLanguage");
        builder.HasKey(e => e.Key);

        builder.Property(e => e.Key).IsRequired();
        builder.Property(e => e.Name).IsRequired();
    }
}
