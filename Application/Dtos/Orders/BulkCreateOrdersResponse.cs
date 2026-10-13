namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Result of a bulk order creation operation, including a per-order outcome.
/// </summary>
public class BulkCreateOrdersResponse
{
    /// <summary>
    /// Gets or sets the number of orders that were created successfully.
    /// </summary>
    public int SuccessfulCount { get; set; }

    /// <summary>
    /// Gets or sets the number of orders that failed validation or persistence.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the per-order results in the same order as the input list.
    /// </summary>
    public IReadOnlyList<BulkOrderResultItem> Results { get; set; } = Array.Empty<BulkOrderResultItem>();
}
