namespace OrderManagement.Domain.Repositories;

/// <summary>
/// نتیجه‌ی یک پرس‌وجوی صفحه‌بندی‌شده‌ی مخزن را حمل می‌کند: یک صفحه اقلام به‌علاوه‌ی شمار کل.
/// در دامنه تعریف شده است چون <c>PagedResult</c> لایه‌ی Application حاوی DTOهاست
/// و دامنه نباید به Application وابستگی داشته باشد.
/// </summary>
/// <typeparam name="TEntity">نوع موجودیتی که پرس‌وجو برمی‌گرداند.</typeparam>
public class PagedEntities<TEntity>
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="PagedEntities{TEntity}"/> می‌سازد.
    /// </summary>
    /// <param name="items">اقلام صفحه‌ی درخواست‌شده.</param>
    /// <param name="totalCount">تعداد کل موجودیت‌های منطبق در همه‌ی صفحات.</param>
    public PagedEntities(IReadOnlyList<TEntity> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }

    /// <summary>
    /// اقلام تعلق‌دار به صفحه‌ی درخواست‌شده را برمی‌گرداند.
    /// </summary>
    public IReadOnlyList<TEntity> Items { get; }

    /// <summary>
    /// تعداد کل موجودیت‌های منطبق در همه‌ی صفحات را برمی‌گرداند.
    /// </summary>
    public int TotalCount { get; }
}
