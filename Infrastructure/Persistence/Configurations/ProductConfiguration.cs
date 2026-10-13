using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Product"/> to the <c>Products</c> table: unique SKU, decimal price,
/// non-negative price/stock check constraints and the row version.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", table =>
        {
            // Invariants are re-checked by the database so no code path can bypass them.
            table.HasCheckConstraint("CK_Products_Price", "Price >= 0");
            table.HasCheckConstraint("CK_Products_StockQuantity", "StockQuantity >= 0");
        });

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name).IsRequired().HasMaxLength(200);
        builder.Property(product => product.Description).HasMaxLength(1000);
        builder.Property(product => product.Sku).IsRequired().HasMaxLength(64);
        builder.Property(product => product.Price).HasColumnType("decimal(18,2)");
        builder.Property(product => product.StockQuantity).IsRequired();
        builder.Property(product => product.IsActive).IsRequired();
        builder.Property(product => product.CreatedAt).IsRequired();
        builder.Property(product => product.UpdatedAt).IsRequired();

        // SKU uniqueness is enforced by the database, mirroring the service duplicate guard.
        builder.HasIndex(product => product.Sku).IsUnique();

        builder.Property(product => product.RowVersion).IsRowVersion();
    }
}
