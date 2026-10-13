using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// <see cref="Product"/> را روی جدول <c>Products</c> نگاشت می‌کند: SKU یکتا، قیمت اعشاری،
/// قیدهای check برای قیمت/موجودی غیرمنفی و نسخه‌ی ردیف.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", table =>
        {
            // ثابت‌ها توسط دیتابیس دوباره بررسی می‌شوند تا هیچ مسیر کدی نتواند آن‌ها را دور بزند.
            table.HasCheckConstraint("CK_Products_Price", "Price >= 0");
            table.HasCheckConstraint("CK_Products_StockQuantity", "StockQuantity >= 0");
        });

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name).IsRequired().HasMaxLength(200);
        builder.Property(product => product.Description).HasMaxLength(1000);
        builder.Property(product => product.Sku).IsRequired().HasMaxLength(64);
        builder.Property(product => product.Price).HasColumnType("decimal(18,2)");
        builder.Property(product => product.StockQuantity).IsRequired();
        builder.Property(product => product.IsActive).IsRequired();
        builder.Property(product => product.CreatedAt).IsRequired();
        builder.Property(product => product.UpdatedAt).IsRequired();

        // یکتایی SKU توسط دیتابیس اعمال می‌شود، هم‌سان با نگهبان تکرار سرویس.
        builder.HasIndex(product => product.Sku).IsUnique();

        // ایندکس پشتیبان فیلتر «بر اساس دسته» در فهرست محصولات.
        builder.HasIndex(product => product.CategoryId);

        // اتصال دسته اختیاری است؛ حذف دسته‌ای که محصول دارد restrict می‌شود
        // (هم‌سان با سیاست موجود برای کلیدهای خارجی غیرcascade در این دیتابیس).
        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(product => product.RowVersion).IsRowVersion();
    }
}
