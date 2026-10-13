using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Thrown when a requested order status transition is not allowed by the business rules.
/// </summary>
public class InvalidStatusTransitionException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStatusTransitionException"/> class.
    /// </summary>
    /// <param name="currentStatus">The status the order currently has.</param>
    /// <param name="requestedStatus">The status that was requested.</param>
    public InvalidStatusTransitionException(OrderStatus currentStatus, OrderStatus requestedStatus)
        : base($"Transition from '{currentStatus}' to '{requestedStatus}' is not allowed.")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }

    /// <summary>
    /// Gets the status the order currently has.
    /// </summary>
    public OrderStatus CurrentStatus { get; }

    /// <summary>
    /// Gets the status that was requested.
    /// </summary>
    public OrderStatus RequestedStatus { get; }
}
