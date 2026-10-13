using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Categories;

/// <summary>
/// بدنه‌ی درخواست برای ایجاد یک دسته‌بندی جدید.
/// </summary>
public class CreateCategoryRequest
{
    /// <summary>
    /// نام نمایشی دسته را می‌گیرد یا تنظیم می‌کند. باید یکتا باشد.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات اختیاری دسته را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// مقداری که نشان می‌دهد دسته فعال است را می‌گیرد یا تنظیم می‌کند. مقدار پیش‌فرض <c>true</c> است.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
