using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// <see cref="Category"/> را روی جدول <c>Categories</c> نگاشت می‌کند: نام و توضیحات،
/// پرچم فعال بودن، ایندکس یکتای نام و نسخه‌ی ردیف برای همزمانی خوش‌بینانه.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name).IsRequired().HasMaxLength(200);
        builder.Property(category => category.Description).HasMaxLength(1000);
        builder.Property(category => category.IsActive).IsRequired();
        builder.Property(category => category.CreatedAt).IsRequired();
        builder.Property(category => category.UpdatedAt).IsRequired();

        // یکتایی نام توسط دیتابیس اعمال می‌شود؛ بررسی تکرار در سرویس فقط یک نگهبان
        // تلاش‌محور برای پیام‌های خطای خوش‌زبان است.
        builder.HasIndex(category => category.Name).IsUnique();

        // توکن همزمانی rowversion: هر به‌روزرسانی همزمان هنگام ذخیره شناسایی می‌شود.
        builder.Property(category => category.RowVersion).IsRowVersion();
    }
}
