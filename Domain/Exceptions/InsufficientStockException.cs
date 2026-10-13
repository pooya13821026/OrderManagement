namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Thrown when an order cannot be confirmed because stock is insufficient for one of its items.
/// No stock is decremented when this exception is raised.
/// </summary>
public class InsufficientStockException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientStockException"/> class.
    /// </summary>
    /// <param name="productId">Identifier of the product with insufficient stock.</param>
    /// <param name="available">Number of units currently available in stock.</param>
    /// <param name="requested">Number of units that were requested.</param>
    public InsufficientStockException(Guid productId, int available, int requested)
        : base($"Insufficient stock for product '{productId}': {available} available, {requested} requested.")
    {
        ProductId = productId;
        Available = available;
        Requested = requested;
    }

    /// <summary>
    /// Gets the identifier of the product with insufficient stock.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Gets the number of units currently available in stock.
    /// </summary>
    public int Available { get; }

    /// <summary>
    /// Gets the number of units that were requested.
    /// </summary>
    public int Requested { get; }
}
