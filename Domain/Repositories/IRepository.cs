namespace OrderManagement.Domain.Repositories;

/// <summary>
/// قرارداد مخزن (repository) عمومی که عملیات پایه‌ی CRUD را برای یک نوع موجودیت فراهم می‌کند.
/// پیاده‌سازی‌ها در لایه‌ی Infrastructure قرار دارند.
/// </summary>
/// <typeparam name="TEntity">نوع موجودیت.</typeparam>
/// <typeparam name="TKey">نوع کلید اصلی موجودیت.</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// موجودیت را بر اساس شناسه‌اش برمی‌گرداند، یا اگر وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="id">شناسه‌ی موجودیت.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// موجودیت جدیدی را به واحد کار (unit of work) اضافه می‌کند. تا زمان فراخوانی ذخیره، تغییرات ماندگار نمی‌شوند.
    /// </summary>
    /// <param name="entity">موجودیتی که باید اضافه شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// مجموعه‌ای از موجودیت‌های جدید را به واحد کار اضافه می‌کند. تا زمان فراخوانی ذخیره، تغییرات ماندگار نمی‌شوند.
    /// </summary>
    /// <param name="entities">موجودیت‌هایی که باید اضافه شوند.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// موجودیت موجود را به‌عنوان تغییریافته علامت می‌زند.
    /// </summary>
    /// <param name="entity">موجودیتی که باید به‌روزرسانی شود.</param>
    void Update(TEntity entity);

    /// <summary>
    /// موجودیت موجود را برای حذف علامت می‌زند.
    /// </summary>
    /// <param name="entity">موجودیتی که باید حذف شود.</param>
    void Remove(TEntity entity);

    /// <summary>
    /// مجموعه‌ای از موجودیت‌های موجود را برای حذف علامت می‌زند.
    /// </summary>
    /// <param name="entities">موجودیت‌هایی که باید حذف شوند.</param>
    void RemoveRange(IEnumerable<TEntity> entities);
}
