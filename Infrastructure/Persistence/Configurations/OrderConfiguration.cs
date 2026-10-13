using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Order"/> to the <c>Orders</c> table: unique order number, the byte
/// (tinyint) status column, decimal totals, list/filter indexes and the row version.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.OrderNumber).IsRequired().HasMaxLength(50);
        // OrderStatus is a byte enum; store it as tinyint with an explicit byte conversion
        // so the database representation can never drift from the CLR enum.
        builder.Property(order => order.Status).HasConversion<byte>().HasColumnType("tinyint");
        builder.Property(order => order.OrderDate).IsRequired();
        builder.Property(order => order.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(order => order.ShippingAddress).HasMaxLength(500);
        builder.Property(order => order.Notes).HasMaxLength(1000);
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.UpdatedAt).IsRequired();

        // Order numbers must be unique across all orders.
        builder.HasIndex(order => order.OrderNumber).IsUnique();

        // Indexes backing the common list filters (by customer, by status, by date range).
        builder.HasIndex(order => order.CustomerId);
        builder.HasIndex(order => order.Status);
        builder.HasIndex(order => order.OrderDate);

        builder.Property(order => order.RowVersion).IsRowVersion();

        // Every order belongs to exactly one customer; deleting the customer removes
        // their orders as well, so no orphaned orders can remain.
        builder.HasOne(order => order.Customer)
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Customer).IsRequired();
    }
}
