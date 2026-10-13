using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// بدنه‌ی درخواست برای ایجاد یک حساب کاربری جدید.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// آدرس ایمیل حساب جدید را می‌گیرد یا تنظیم می‌کند. یکتاست.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// گذرواژه‌ی متنی ساده را می‌گیرد یا تنظیم می‌کند. پیش از ذخیره هش می‌شود؛ هرگز به همان صورت ذخیره نمی‌شود.
    /// </summary>
    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// نام کاربر را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// نام خانوادگی کاربر را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// شماره تلفن اختیاری کاربر را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
