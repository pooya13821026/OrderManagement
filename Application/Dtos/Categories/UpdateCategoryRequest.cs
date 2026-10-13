using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Categories;

/// <summary>
/// بدنه‌ی درخواست برای به‌روزرسانی یک دسته‌بندی موجود.
/// </summary>
public class UpdateCategoryRequest
{
    /// <summary>
    /// نام نمایشی دسته را می‌گیرد یا تنظیم می‌کند. باید یکتا باقی بماند.
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
    /// مقداری که نشان می‌دهد دسته فعال است را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
