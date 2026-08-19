using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Data.Configurations;

public class CatalogTypeConfiguration : IEntityTypeConfiguration<CatalogType>
{
    public void Configure(EntityTypeBuilder<CatalogType> builder)
    {
        builder.ToTable("CatalogType");

        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Id)
            .UseHiLo("catalog_type_hilo", CatalogContext.Schema)
            .IsRequired();

        builder.Property(ct => ct.Type)
            .IsRequired()
            .HasMaxLength(CatalogType.MaxTypeLength);

        builder.HasIndex(ct => ct.Type).IsUnique();
    }
}
