using Menu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Infrastructure.Data.Configurations;

public class ItemOptionConfiguration : IEntityTypeConfiguration<ItemOption>
{
    public void Configure(EntityTypeBuilder<ItemOption> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SelectionType).HasConversion<int>();
        builder.HasIndex(x => new { x.MenuItemId, x.Name });

        builder.HasMany(x => x.Values)
            .WithOne(x => x.ItemOption)
            .HasForeignKey(x => x.ItemOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
