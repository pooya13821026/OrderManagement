namespace OrderManagement.Domain.Repositories;

/// <summary>
/// Carries the result of a paged repository query: one page of items plus the total count.
/// Defined in the domain because the Application layer's <c>PagedResult</c> contains DTOs
/// and the Domain must not depend on Application.
/// </summary>
/// <typeparam name="TEntity">The entity type returned by the query.</typeparam>
public class PagedEntities<TEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PagedEntities{TEntity}"/> class.
    /// </summary>
    /// <param name="items">The items of the requested page.</param>
    /// <param name="totalCount">The total number of matching entities across all pages.</param>
    public PagedEntities(IReadOnlyList<TEntity> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }

    /// <summary>
    /// Gets the items belonging to the requested page.
    /// </summary>
    public IReadOnlyList<TEntity> Items { get; }

    /// <summary>
    /// Gets the total number of matching entities across all pages.
    /// </summary>
    public int TotalCount { get; }
}
