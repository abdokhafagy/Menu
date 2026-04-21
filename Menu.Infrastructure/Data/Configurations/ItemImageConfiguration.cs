using Menu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Infrastructure.Data.Configurations;

public class ItemImageConfiguration : IEntityTypeConfiguration<ItemImage>
{
    public void Configure(EntityTypeBuilder<ItemImage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
