using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Data.Configurations;

public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.ToTable("CatalogItem");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .UseHiLo("catalog_hilo", CatalogContext.Schema)
            .IsRequired();

        builder.Property(ci => ci.Name)
            .IsRequired()
            .HasMaxLength(CatalogItem.MaxNameLength);

        builder.Property(ci => ci.Description)
            .IsRequired()
            .HasMaxLength(CatalogItem.MaxDescriptionLength);

        // Money is an owned value object flattened onto the item's own row.
        builder.OwnsOne(ci => ci.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("Price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            price.Property(p => p.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();
        });
        builder.Navigation(ci => ci.Price).IsRequired();

        builder.Property(ci => ci.PictureFileName)
            .IsRequired()
            .HasMaxLength(CatalogItem.MaxPictureFileNameLength);

        builder.Property(ci => ci.PictureUri)
            .IsRequired()
            .HasMaxLength(CatalogItem.MaxPictureUriLength);

        builder.Property(ci => ci.AvailableStock).IsRequired();
        builder.Property(ci => ci.RestockThreshold).IsRequired();
        builder.Property(ci => ci.MaxStockThreshold).IsRequired();
        builder.Property(ci => ci.OnReorder).IsRequired();

        // Guards concurrent stock changes: two simultaneous RemoveStock calls
        // must not both succeed against the same starting quantity.
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.Ignore(ci => ci.DomainEvents);

        builder.HasOne(ci => ci.CatalogBrand)
            .WithMany()
            .HasForeignKey(ci => ci.CatalogBrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ci => ci.CatalogType)
            .WithMany()
            .HasForeignKey(ci => ci.CatalogTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ci => new { ci.CatalogBrandId, ci.CatalogTypeId });
        builder.HasIndex(ci => ci.Name);
    }
}
