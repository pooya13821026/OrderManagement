using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Customers;

/// <summary>
/// بدنه‌ی درخواست برای ایجاد یک مشتری جدید.
/// </summary>
public class CreateCustomerRequest
{
    /// <summary>
    /// نام مشتری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// نام خانوادگی مشتری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// آدرس ایمیل یکتای مشتری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// شماره تلفن اختیاری مشتری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// آدرس پستی اختیاری مشتری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
}
