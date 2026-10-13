using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="ApplicationUser"/> to the <c>Users</c> table: unique email and user
/// name, required credential fields and the row version.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.UserName).IsRequired().HasMaxLength(256);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(256);
        // BCrypt hashes stay well below this limit; the cap only protects the column size.
        builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(user => user.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.LastName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.PhoneNumber).HasMaxLength(50);
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.Role).IsRequired().HasMaxLength(64);
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.SecurityStamp).IsRequired().HasMaxLength(128);

        // Email and user name are both unique: sign-in must never be ambiguous.
        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.UserName).IsUnique();

        builder.Property(user => user.RowVersion).IsRowVersion();
    }
}
