namespace OrderManagement.Domain.Enums;

/// <summary>
/// Lifecycle status of an order. Stored as a byte in the database.
/// </summary>
public enum OrderStatus : byte
{
    /// <summary>
    /// Order has been placed but not yet confirmed. Stock is not yet reserved.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Order has been confirmed and stock has been decremented.
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// Order has been handed to the carrier.
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// Order has been delivered to the customer. Terminal success state.
    /// </summary>
    Delivered = 4,

    /// <summary>
    /// Order has been cancelled. Terminal state; only allowed from Pending or Confirmed.
    /// </summary>
    Cancelled = 5
}
