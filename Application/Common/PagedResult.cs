namespace OrderManagement.Application.Common;

/// <summary>
/// Represents one page of a paged query together with pagination metadata.
/// </summary>
/// <typeparam name="T">The type of the items on the page.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PagedResult{T}"/> class.
    /// </summary>
    /// <param name="items">The items belonging to the requested page.</param>
    /// <param name="pageNumber">The 1-based page number that was requested.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="totalCount">The total number of matching items across all pages.</param>
    public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = pageSize > 0
            ? (int)Math.Ceiling(totalCount / (double)pageSize)
            : 0;
    }

    /// <summary>
    /// Gets the items belonging to the requested page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Gets the 1-based page number that was requested.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of matching items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the total number of pages available for the given page size.
    /// </summary>
    public int TotalPages { get; }
}
