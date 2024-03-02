using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OliverBooth.Common.Data.ValueConverters;
using OliverBooth.Common.Data.Web.Users;

namespace OliverBooth.Common.Data.Web.Configuration;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.EmailAddress).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Password).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Salt).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Registered).IsRequired();
        builder.Property(e => e.Totp);
        builder.Property(e => e.Permissions).HasConversion<PermissionListConverter>();
    }
}
