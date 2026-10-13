using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// بدنه‌ی درخواست برای مبادله‌ی یک توکن refresh معتبر با جفت توکن جدید (rotation).
/// </summary>
public class RefreshRequest
{
    /// <summary>
    /// توکن refresh قبلاً صادرشده برای کلاینت را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string RefreshToken { get; set; } = string.Empty;
}
