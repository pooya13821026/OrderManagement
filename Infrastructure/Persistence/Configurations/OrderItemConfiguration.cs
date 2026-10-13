using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// <see cref="OrderItem"/> را روی جدول <c>OrderItems</c> نگاشت می‌کند: قید check برای
/// تعداد مثبت، قیمت واحد اعشاری، حذف cascade با سفارش و حذف restrict با محصول،
/// به‌علاوه‌ی نسخه‌ی ردیف.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", table =>
        {
            // ردیف سفارش بدون تعداد معنا ندارد؛ نگهبان دامنه توسط دیتابیس پشتیبانی می‌شود.
            table.HasCheckConstraint("CK_OrderItems_Quantity", "Quantity > 0");
        });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Quantity).IsRequired();
        builder.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(item => item.CreatedAt).IsRequired();
        builder.Property(item => item.RowVersion).IsRowVersion();

        // LineTotal یک پروپرتی محاسباتی (Quantity * UnitPrice) است و نباید به ستون تبدیل شود.
        builder.Ignore(item => item.LineTotal);

        // اقلام بدون سفارششان بی‌ارزشند: حذف سفارش، اقلامش را هم حذف می‌کند.
        builder.HasOne(item => item.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // محصولی که توسط یک ردیف سفارش ارجاع شده هرگز نباید از این مسیر حذف شود؛
        // سفارش‌ها اسنپ‌شات قیمت خود را حتی پس از بازنشستگی ردیف کاتالوگ نگه می‌دارند.
        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
