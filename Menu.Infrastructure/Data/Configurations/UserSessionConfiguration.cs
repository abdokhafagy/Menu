using Menu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Infrastructure.Data.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Jti).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RefreshTokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.RefreshTokenHash);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
