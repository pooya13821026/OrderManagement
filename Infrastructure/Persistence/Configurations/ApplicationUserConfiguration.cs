using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// <see cref="ApplicationUser"/> را روی جدول <c>Users</c> نگاشت می‌کند: ایمیل و نام کاربری
/// یکتا، فیلدهای الزامی اعتبارنامه و نسخه‌ی ردیف.
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
        // هش‌های BCrypt خیلی کمتر از این سقف‌اند؛ این سقف فقط اندازه‌ی ستون را محافظت می‌کند.
        builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(user => user.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.LastName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.PhoneNumber).HasMaxLength(50);
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.Role).IsRequired().HasMaxLength(64);
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.SecurityStamp).IsRequired().HasMaxLength(128);

        // ایمیل و نام کاربری هر دو یکتایند: ورود به سیستم هرگز نباید مبهم باشد.
        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.UserName).IsUnique();

        builder.Property(user => user.RowVersion).IsRowVersion();
    }
}
