namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// هنگامی پرتاب می‌شود که فراخواننده اجازه‌ی انجام عملیات درخواستی را نداشته باشد،
/// برای نمونه وقتی فردی غیر ادمین بخواهد سفارش را لغو کند یا اطلاعات ورود نامعتبر باشد.
/// </summary>
public class UnauthorizedOperationException : DomainException
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="UnauthorizedOperationException"/> می‌سازد.
    /// </summary>
    public UnauthorizedOperationException()
        : base("این عملیات مجاز نیست.")
    {
    }

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="UnauthorizedOperationException"/> می‌سازد.
    /// </summary>
    /// <param name="message">پیامی که خطا را توصیف می‌کند.</param>
    public UnauthorizedOperationException(string message)
        : base(message)
    {
    }
}
