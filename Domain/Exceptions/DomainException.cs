namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// استثنای پایه برای همه‌ی نقض‌های قواعد کسب‌وکار که توسط لایه‌ی دامنه یا برنامه پرتاب می‌شوند.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="DomainException"/> می‌سازد.
    /// </summary>
    /// <param name="message">پیامی که خطا را توصیف می‌کند.</param>
    public DomainException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="DomainException"/> می‌سازد.
    /// </summary>
    /// <param name="message">پیامی که خطا را توصیف می‌کند.</param>
    /// <param name="innerException">استثنایی که باعث این استثنا شده است.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
