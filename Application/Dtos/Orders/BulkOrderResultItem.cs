namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Outcome of a single order inside a bulk creation request.
/// </summary>
public class BulkOrderResultItem
{
    /// <summary>
    /// Gets or sets the zero-based index of the order in the submitted request list.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this order was created successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the created order, or <c>null</c> when creation failed.
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the order number of the created order, or <c>null</c> when creation failed.
    /// </summary>
    public string? OrderNumber { get; set; }

    /// <summary>
    /// Gets or sets the error message when creation failed, or <c>null</c> on success.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
