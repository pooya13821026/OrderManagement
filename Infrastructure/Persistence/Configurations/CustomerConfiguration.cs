using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// <see cref="Customer"/> را روی جدول <c>Customers</c> نگاشت می‌کند: نام و ایمیل الزامی،
/// ایندکس یکتای ایمیل، پیوند اختیاری یک‌به‌یک با حساب کاربری (یکتایی UserId) و نسخه‌ی
/// ردیف برای همزمانی خوش‌بینانه.
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

        // یکتایی ایمیل توسط دیتابیس اعمال می‌شود؛ بررسی تکرار در سرویس فقط یک نگهبان
        // تلاش‌محور برای پیام‌های خطای خوش‌زبان است.
        builder.HasIndex(customer => customer.Email).IsUnique();

        // پیوند اختیاری یک‌به‌یک با حساب کاربری: FK nullable است چون ردیف‌های قدیمی/seed
        // ممکن است بدون حساب ساخته شده باشند؛ حذف حساب، پروفایل مشتری را نگه می‌دارد
        // (SetNull) تا تاریخچه‌ی سفارش‌ها بی‌مشتری نشود.
        builder.HasOne(customer => customer.User)
            .WithMany()
            .HasForeignKey(customer => customer.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ایندکس یکتا روی UserId: هر حساب کاربری حداکثر یک پروفایل مشتری داشته باشد
        // (یک‌به‌یک اختیاری). ردیف‌های بدون پیوند (UserId خالی) در ایندکس یکتای SQL Server
        // تکرار محسوب نمی‌شوند.
        builder.HasIndex(customer => customer.UserId).IsUnique();

        // توکن همزمانی rowversion: هر به‌روزرسانی همزمان هنگام ذخیره شناسایی می‌شود.
        builder.Property(customer => customer.RowVersion).IsRowVersion();
    }
}
