using Menu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Infrastructure.Data.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu.Domain.Entities.Menu>
{
    public void Configure(EntityTypeBuilder<Menu.Domain.Entities.Menu> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.RestaurantId, x.IsActive, x.Name });

        builder.HasMany(x => x.Categories)
            .WithOne(x => x.Menu)
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
