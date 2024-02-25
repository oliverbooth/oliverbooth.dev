using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OliverBooth.Data.Web.Configuration;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Session");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Created).IsRequired();
        builder.Property(e => e.Updated).IsRequired();
        builder.Property(e => e.LastAccessed).IsRequired();
        builder.Property(e => e.Expires).IsRequired();
        builder.Property(e => e.UserAgent).HasMaxLength(255).IsRequired();
        builder.Property(e => e.UserId);
        builder.Property(e => e.IpAddress).HasConversion<IPAddressToBytesConverter>().IsRequired();
        builder.Property(e => e.RequiresTotp).IsRequired();
    }
}
