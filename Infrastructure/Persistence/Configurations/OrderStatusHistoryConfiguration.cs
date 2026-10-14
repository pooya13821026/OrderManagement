using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// <see cref="OrderStatusHistory"/> را روی جدول <c>OrderStatusHistories</c> نگاشت می‌کند:
/// کلید اصلی GUID، کلید خارجی آبشاری به سفارش، وضعیت‌ها به‌صورت byte (tinyint) و
/// ایندکس‌های پرس‌وجوی تاریخچه (بر اساس سفارش و بر اساس زمان گذار).
/// عمداً نسخه‌ی ردیف (RowVersion) ندارد: این جدول append-only است، ردیف هرگز
/// ویرایش یا حذف نمی‌شود پس تداخل نوشتن روی آن ممکن نیست.
/// </summary>
public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");
        builder.HasKey(history => history.Id);

        // OrderStatus ها مانند جدول سفارش به‌صورت byte ذخیره می‌شوند تا نمایش
        // دیتابیس هرگز از enum در CLR فاصله نگیرد.
        builder.Property(history => history.FromStatus).HasConversion<byte>().HasColumnType("tinyint");
        builder.Property(history => history.ToStatus).HasConversion<byte>().HasColumnType("tinyint");
        builder.Property(history => history.ChangedAt).IsRequired();
        builder.Property(history => history.ChangedByRole).HasMaxLength(50);
        builder.Property(history => history.Reason).HasMaxLength(500);

        // ایندکس‌های پرس‌وجوی رایج: تاریخچه‌ی یک سفارش (پرتره‌ی سفارش) و گذارهای یک بازه‌ی زمانی.
        builder.HasIndex(history => history.OrderId);
        builder.HasIndex(history => history.ChangedAt);

        // حذف آبشاری با سفارش: تاریخچه بخشی از همان aggregate سفارش است؛
        // سفارش بدون تاریخچه‌اش (یا تاریخچه‌ی سفارش حذف‌شده) یتیم و بی‌ارزش می‌شود.
        builder.HasOne(history => history.Order)
            .WithMany(order => order.StatusHistory)
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(history => history.Order).IsRequired();
    }
}
