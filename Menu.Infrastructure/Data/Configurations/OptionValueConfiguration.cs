using Menu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Infrastructure.Data.Configurations;

public class OptionValueConfiguration : IEntityTypeConfiguration<OptionValue>
{
    public void Configure(EntityTypeBuilder<OptionValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PriceModifier).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.ItemOptionId, x.DisplayOrder });
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
