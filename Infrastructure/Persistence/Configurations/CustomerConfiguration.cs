using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Customer"/> to the <c>Customers</c> table: required name and email,
/// a unique email index and the optimistic-concurrency row version.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(customer => customer.LastName).IsRequired().HasMaxLength(100);
        builder.Property(customer => customer.Email).IsRequired().HasMaxLength(256);
        builder.Property(customer => customer.Phone).HasMaxLength(50);
        builder.Property(customer => customer.Address).HasMaxLength(500);
        builder.Property(customer => customer.CreatedAt).IsRequired();
        builder.Property(customer => customer.UpdatedAt).IsRequired();

        // Email uniqueness is enforced by the database; the service-level duplicate
        // check is only a best effort guard against friendly error messages.
        builder.HasIndex(customer => customer.Email).IsUnique();

        // rowversion concurrency token: any concurrent update is detected on save.
        builder.Property(customer => customer.RowVersion).IsRowVersion();
    }
}
