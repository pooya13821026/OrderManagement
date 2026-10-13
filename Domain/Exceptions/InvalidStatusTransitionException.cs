using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// هنگامی پرتاب می‌شود که گذار وضعیت سفارش درخواستی، توسط قواعد کسب‌وکار مجاز نباشد.
/// </summary>
public class InvalidStatusTransitionException : DomainException
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="InvalidStatusTransitionException"/> می‌سازد.
    /// </summary>
    /// <param name="currentStatus">وضعیت فعلی سفارش.</param>
    /// <param name="requestedStatus">وضعیت درخواست‌شده.</param>
    public InvalidStatusTransitionException(OrderStatus currentStatus, OrderStatus requestedStatus)
        : base($"گذار از '{currentStatus}' به '{requestedStatus}' مجاز نیست.")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }

    /// <summary>
    /// وضعیت فعلی سفارش را برمی‌گرداند.
    /// </summary>
    public OrderStatus CurrentStatus { get; }

    /// <summary>
    /// وضعیت درخواست‌شده را برمی‌گرداند.
    /// </summary>
    public OrderStatus RequestedStatus { get; }
}
