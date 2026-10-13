using OrderManagement.Application.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Filter and pagination inputs for paging through orders.
/// </summary>
public class OrderFilterParams : PaginationParams
{
    /// <summary>
    /// Gets or sets an optional customer identifier filter.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets an optional order status filter.
    /// </summary>
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets an optional inclusive lower bound on the order date (UTC).
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets an optional inclusive upper bound on the order date (UTC).
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Gets or sets an optional free-text search term matched against order number
    /// and shipping address.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the optional sort column: <c>OrderNumber</c>, <c>OrderDate</c>
    /// or <c>TotalAmount</c>. <c>null</c> falls back to order date.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether sorting should be descending
    /// instead of ascending.
    /// </summary>
    public bool SortDescending { get; set; }
}
