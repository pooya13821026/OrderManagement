using OrderManagement.Application.Interfaces;

namespace OrderManagement.Infrastructure.Security;

/// <summary>
/// هش‌کردن یک‌طرفه‌ی گذرواژه با BCrypt که <see cref="IPasswordHasher"/> را پیاده‌سازی می‌کند.
/// BCrypt یک salt مخصوص هر گذرواژه را در هش تعبیه می‌کند، بنابراین گذرواژه‌های یکسان هرگز
/// هش‌های یکسان تولید نمی‌کنند.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// ضریب کار BCrypt: مقدار ۱۱ هش‌کردن را به‌راحتی بالاتر از سرعت brute-force تعاملی نگه می‌دارد
    /// و در عین حال خیلی کمتر از بودجه‌ی تأخیر یک درخواست ورود باقی می‌ماند.
    /// </summary>
    private const int WorkFactor = 11;

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <inheritdoc />
    public bool Verify(string password, string passwordHash)
    {
        // ورودی خالی هرگز نمی‌تواند با هش ذخیره‌شده مطابقت کند؛ برگرداندن false مسیر
        // «اطلاعات ورود نامعتبر» فراخواننده را بدون فاش کردن دلیل شکست حفظ می‌کند.
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // یک هش ذخیره‌شده‌ی خراب به جای از کار انداختن ورود، عدم مطابق در نظر گرفته می‌شود.
            return false;
        }
    }
}
