using OrderManagement.Application.Dtos.Auth;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد سرویس برنامه برای احراز هویت: ثبت‌نام، ورود،
/// چرخه‌ی توکن refresh و ابطال آن.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// کاربر جدیدی را با نقش پیش‌فرض <c>User</c> ثبت می‌کند، گذرواژه را هش می‌کند
    /// و جفت توکن تازه‌ای برمی‌گرداند.
    /// </summary>
    /// <param name="request">بدنه‌ی درخواست ثبت‌نام.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>پاسخ احراز هویت شامل توکن‌ها و پروفایل کاربر.</returns>
    /// <exception cref="Domain.Exceptions.DuplicateEntityException">وقتی ایمیل قبلاً ثبت شده باشد پرتاب می‌شود.</exception>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// با ایمیل و گذرواژه وارد می‌شود، زمان‌نشان آخرین ورود را به‌روزرسانی می‌کند
    /// و جفت توکن تازه‌ای برمی‌گرداند.
    /// </summary>
    /// <param name="request">بدنه‌ی درخواست ورود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>پاسخ احراز هویت شامل توکن‌ها و پروفایل کاربر.</returns>
    /// <exception cref="Domain.Exceptions.UnauthorizedOperationException">
    /// وقتی اطلاعات ورود اشتباه باشد یا حساب غیرفعال شده باشد پرتاب می‌شود.
    /// </exception>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// توکن refresh معتبر، ابطال‌نشده و منقضی‌نشده را با جفت توکن جدید مبادله می‌کند
    /// و توکن قدیمی را باطل می‌کند (rotation).
    /// </summary>
    /// <param name="request">بدنه‌ی درخواست refresh شامل توکن refresh فعلی.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>پاسخ احراز هویت با توکن‌های جدید و پروفایل کاربر.</returns>
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// توکن refresh داده‌شده را باطل می‌کند تا دیگر قابل مبادله نباشد.
    /// </summary>
    /// <param name="refreshToken">توکن refresh خامی که باید باطل شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی هیچ توکن ذخیره‌شده‌ای منطبق نباشد پرتاب می‌شود.</exception>
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// پروفایل کاربر در حال حاضر احراز هویت‌شده را برمی‌گرداند.
    /// </summary>
    /// <param name="userId">شناسه‌ی کاربر فعلی.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>پروفایل کاربر به‌صورت DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی کاربر وجود نداشته باشد پرتاب می‌شود.</exception>
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
