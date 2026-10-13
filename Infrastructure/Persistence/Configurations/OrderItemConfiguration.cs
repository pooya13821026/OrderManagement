using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="OrderItem"/> to the <c>OrderItems</c> table: positive quantity check
/// constraint, decimal unit price, cascade delete with the order and restrict delete
/// with the product, plus the row version.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", table =>
        {
            // A line item without units makes no sense; the domain guard is backed by the database.
            table.HasCheckConstraint("CK_OrderItems_Quantity", "Quantity > 0");
        });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Quantity).IsRequired();
        builder.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(item => item.CreatedAt).IsRequired();
        builder.Property(item => item.RowVersion).IsRowVersion();

        // LineTotal is a computed property (Quantity * UnitPrice) and must not become a column.
        builder.Ignore(item => item.LineTotal);

        // Items are worthless without their order: deleting an order deletes its items.
        builder.HasOne(item => item.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // A product referenced by an order item must never be deleted through this path;
        // orders keep their price snapshots even when the catalog entry is retired.
        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
