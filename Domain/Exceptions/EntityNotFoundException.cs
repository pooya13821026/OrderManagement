namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// هنگامی پرتاب می‌شود که موجودیتی با شناسه‌ی داده‌شده وجود نداشته باشد.
/// </summary>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="EntityNotFoundException"/> می‌سازد.
    /// </summary>
    /// <param name="entityName">نام ساده‌ی نوع موجودیت، برای نمونه <c>Order</c>.</param>
    /// <param name="id">شناسه‌ای که پیدا نشد.</param>
    public EntityNotFoundException(string entityName, object id)
        : base($"رکورد {entityName} با شناسه '{id}' یافت نشد.")
    {
        EntityName = entityName;
        Id = id;
    }

    /// <summary>
    /// نام ساده‌ی نوع موجودیتی که پیدا نشد را برمی‌گرداند.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// شناسه‌ای که پیدا نشد را برمی‌گرداند.
    /// </summary>
    public object Id { get; }
}
