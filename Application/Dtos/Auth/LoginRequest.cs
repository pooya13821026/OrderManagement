using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// بدنه‌ی درخواست برای ورود با ایمیل و گذرواژه.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// آدرس ایمیل حساب را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// گذرواژه‌ی متنی ساده‌ای که باید تأیید شود را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
