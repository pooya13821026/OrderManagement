namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// هنگامی پرتاب می‌شود که ایجاد یا به‌روزرسانی یک موجودیت، قید یکتایی را نقض کند،
/// برای نمونه تکراری بودن ایمیل یا کد SKU.
/// </summary>
public class DuplicateEntityException : DomainException
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="DuplicateEntityException"/> را با پیام پیش‌فرض
    /// برای موجودیت داده‌شده می‌سازد.
    /// </summary>
    /// <param name="entityName">نام ساده‌ی نوع موجودیت، برای نمونه <c>Customer</c>.</param>
    public DuplicateEntityException(string entityName)
        : base($"{entityName} از قبل وجود دارد.")
    {
        EntityName = entityName;
    }

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="DuplicateEntityException"/> را با پیام سفارشی می‌سازد.
    /// </summary>
    /// <param name="entityName">نام ساده‌ی نوع موجودیت، برای نمونه <c>Customer</c>.</param>
    /// <param name="message">پیامی که خطا را توصیف می‌کند.</param>
    public DuplicateEntityException(string entityName, string message)
        : base(message)
    {
        EntityName = entityName;
    }

    /// <summary>
    /// نام ساده‌ی نوع موجودیتی که باعث تعارض شده را برمی‌گرداند.
    /// </summary>
    public string EntityName { get; }
}
