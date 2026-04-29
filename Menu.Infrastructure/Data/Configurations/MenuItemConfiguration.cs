using Menu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Infrastructure.Data.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.CategoryId, x.IsAvailable, x.DisplayOrder });

        builder.HasMany(x => x.Options)
            .WithOne(x => x.MenuItem)
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.MenuItem)
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
