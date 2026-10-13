using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// قرارداد مخزن برای موجودیت‌های <see cref="Order"/>.
/// </summary>
public interface IOrderRepository : IRepository<Order, Guid>
{
    /// <summary>
    /// سفارش را به همراه مشتری و همه‌ی اقلام با محصولاتشان برمی‌گرداند،
    /// یا اگر چنین سفارشی وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="id">شناسه‌ی سفارش.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// سفارش را بر اساس شماره‌ی یکتای آن برمی‌گرداند، یا اگر چنین سفارشی وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="orderNumber">شماره‌ی سفارشی که باید جست‌وجو شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای از سفارش‌ها را با فیلترهای اختیاری برای مشتری، وضعیت، بازه‌ی تاریخ و یک عبارت
    /// جست‌وجوی متنی آزاد که روی شماره سفارش و آدرس ارسال تطبیق داده می‌شود، به همراه مرتب‌سازی
    /// قابل پیکربندی برمی‌گرداند.
    /// </summary>
    /// <param name="customerId">فیلتر اختیاری شناسه‌ی مشتری.</param>
    /// <param name="status">فیلتر اختیاری وضعیت سفارش.</param>
    /// <param name="fromDate">کران پایین اختیاری و شامل بر تاریخ سفارش.</param>
    /// <param name="toDate">کران بالای اختیاری و شامل بر تاریخ سفارش.</param>
    /// <param name="searchTerm">عبارت جست‌وجوی متنی آزاد اختیاری؛ <c>null</c> یا خالی فیلتر متن را رد می‌کند.</param>
    /// <param name="sortBy">نام ستون مرتب‌سازی اختیاری؛ <c>null</c> به مرتب‌سازی پیش‌فرض (تاریخ سفارش) برمی‌گردد.</param>
    /// <param name="sortDescending">اینکه مرتب‌سازی نزولی باشد به جای صعودی.</param>
    /// <param name="pageNumber">شماره‌ی صفحه که از ۱ شروع می‌شود.</param>
    /// <param name="pageSize">تعداد اقلام در هر صفحه.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<PagedEntities<Order>> GetPagedAsync(
        Guid? customerId,
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تعیین می‌کند که آیا سفارشی با این شماره سفارش از قبل وجود دارد یا نه.
    /// </summary>
    /// <param name="orderNumber">شماره‌ی سفارشی که باید بررسی شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<bool> ExistsByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
}
