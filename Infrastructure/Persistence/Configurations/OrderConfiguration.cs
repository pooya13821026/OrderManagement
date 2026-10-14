using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// <see cref="Order"/> را روی جدول <c>Orders</c> نگاشت می‌کند: شماره‌ی سفارش یکتا، ستون
/// وضعیت byte (tinyint)، مبالغ اعشاری، زمان‌نشان‌های چرخه‌ی حیات و دلیل لغو،
/// ایندکس‌های فهرست/فیلتر، نسخه‌ی ردیف و ناوبری تاریخچه‌ی وضعیت.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.OrderNumber).IsRequired().HasMaxLength(50);
        // OrderStatus یک enum از نوع byte است؛ آن را به‌صورت tinyint با تبدیل صریح byte
        // ذخیره می‌کنیم تا نمایش در دیتابیس هرگز از enum در CLR فاصله نگیرد.
        builder.Property(order => order.Status).HasConversion<byte>().HasColumnType("tinyint");
        builder.Property(order => order.OrderDate).IsRequired();
        builder.Property(order => order.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(order => order.ShippingAddress).HasMaxLength(500);
        builder.Property(order => order.Notes).HasMaxLength(1000);
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.UpdatedAt).IsRequired();

        // زمان‌نشان‌های اختیاری چرخه‌ی حیات: تا پیش از رخدادِ مربوطه null می‌مانند.
        builder.Property(order => order.ConfirmedAt);
        builder.Property(order => order.ShippedAt);
        builder.Property(order => order.DeliveredAt);
        builder.Property(order => order.CancelledAt);
        builder.Property(order => order.CancelReason).HasMaxLength(500);

        // شماره‌های سفارش باید در میان همه‌ی سفارش‌ها یکتا باشند.
        builder.HasIndex(order => order.OrderNumber).IsUnique();

        // ایندکس‌های پشتیبان فیلترهای رایج فهرست (بر اساس مشتری، بر اساس وضعیت، بر اساس بازه‌ی تاریخ).
        builder.HasIndex(order => order.CustomerId);
        builder.HasIndex(order => order.Status);
        builder.HasIndex(order => order.OrderDate);

        builder.Property(order => order.RowVersion).IsRowVersion();

        // هر سفارش دقیقاً به یک مشتری تعلق دارد؛ حذف مشتری سفارش‌های او را هم حذف
        // می‌کند، بنابراین سفارش یتیمی باقی نمی‌ماند.
        builder.HasOne(order => order.Customer)
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Customer).IsRequired();

        // تاریخچه‌ی وضعیت عضوی از aggregate سفارش است؛ حذف سفارش، ردیف‌های
        // تاریخچه‌ی آن را هم آبشاری حذف می‌کند تا سفارشی بدون تاریخچه باقی نماند.
        builder.HasMany(order => order.StatusHistory)
            .WithOne(history => history.Order)
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
