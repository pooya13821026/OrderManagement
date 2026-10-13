namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد هش‌کردن یک‌طرفه‌ی گذرواژه و تأیید آن.
/// الگوریتم مشخص (برای نمونه BCrypt) توسط لایه‌ی Infrastructure فراهم می‌شود.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// گذرواژه‌ی متنی ساده را هش می‌کند.
    /// </summary>
    /// <param name="password">گذرواژه‌ی متنی ساده.</param>
    /// <returns>هش گذرواژه برای ذخیره‌سازی.</returns>
    string Hash(string password);

    /// <summary>
    /// گذرواژه‌ی متنی ساده را با هش محاسبه‌شده‌ی قبلی تأیید می‌کند.
    /// </summary>
    /// <param name="password">گذرواژه‌ی متنی ساده‌ای که باید بررسی شود.</param>
    /// <param name="passwordHash">هش گذرواژه‌ی ذخیره‌شده.</param>
    /// <returns><c>true</c> اگر گذرواژه با هش مطابقت داشته باشد؛ در غیر این صورت <c>false</c>.</returns>
    bool Verify(string password, string passwordHash);
}
