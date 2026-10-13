namespace OrderManagement.Domain.Entities;

/// <summary>
/// نماینده‌ی یک توکن refresh ذخیره‌شده است. فقط هش توکن ذخیره می‌شود؛
/// توکن خام دقیقاً یک بار هنگام صدور به کلاینت برمی‌گردد.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// شناسه‌ی یکتای رکورد توکن refresh ذخیره‌شده.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// شناسه‌ی کاربری که توکن به او تعلق دارد.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// هش توکن refresh صادرشده. در میان همه‌ی رکوردها یکتاست.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// زمان UTC که پس از آن توکن دیگر معتبر نیست.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// زمان UTC هنگام ایجاد رکورد توکن.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان UTC هنگام ابطال توکن، یا <c>null</c> تا زمانی که همچنان فعال است.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// هش توکنی که هنگام چرخه (rotation) جایگزین این توکن شده، یا <c>null</c> اگر چرخه‌ای رخ نداده باشد.
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    /// <summary>
    /// مقداری برمی‌گرداند که نشان می‌دهد توکن در حال حاضر قابل استفاده است
    /// (هرگز ابطال نشده و هنگام انقضا نرسیده است).
    /// </summary>
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// پروپرتی ناوبری به حساب کاربری مالک توکن.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
