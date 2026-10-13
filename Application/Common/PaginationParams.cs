using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Common;

/// <summary>
/// Standard pagination inputs used by all paged list endpoints.
/// Call <see cref="Normalize"/> before passing the values to a repository
/// so out-of-range values are clamped instead of causing errors.
/// </summary>
public class PaginationParams
{
    /// <summary>
    /// Default page size used when no (or an invalid) page size is supplied.
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Upper bound for <see cref="PageSize"/>. Requested sizes above this value are clamped.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Gets or sets the 1-based page number. Defaults to 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page. Defaults to <see cref="DefaultPageSize"/>,
    /// clamped to <see cref="MaxPageSize"/>.
    /// </summary>
    [Range(1, MaxPageSize)]
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>
    /// Clamps <see cref="PageNumber"/> and <see cref="PageSize"/> to valid ranges:
    /// page number at least 1, page size between 1 and <see cref="MaxPageSize"/>.
    /// </summary>
    public void Normalize()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (PageSize < 1)
        {
            PageSize = DefaultPageSize;
        }
        else if (PageSize > MaxPageSize)
        {
            PageSize = MaxPageSize;
        }
    }
}
