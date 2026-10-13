namespace OrderManagement.Application.Dtos.Categories;

/// <summary>
/// نمای عمومی یک دسته‌بندی محصول.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// شناسه‌ی یکتای دسته را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// نام نمایشی دسته را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات اختیاری دسته را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// مقداری که نشان می‌دهد دسته فعال است را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد دسته را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین به‌روزرسانی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
