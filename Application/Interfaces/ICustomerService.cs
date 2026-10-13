using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Customers;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد سرویس برنامه برای مدیریت مشتریان.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// پس از بررسی اینکه ایمیل اشغال نشده باشد، مشتری جدیدی ایجاد می‌کند.
    /// </summary>
    /// <param name="request">بدنه‌ی درخواست ایجاد مشتری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>مشتری ایجادشده به‌صورت DTO.</returns>
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// مشتری را بر اساس شناسه برمی‌گرداند.
    /// </summary>
    /// <param name="id">شناسه‌ی مشتری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>مشتری به‌صورت DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی مشتری وجود نداشته باشد پرتاب می‌شود.</exception>
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای فیلترشده و صفحه‌بندی‌شده از مشتریان برمی‌گرداند.
    /// </summary>
    /// <param name="parameters">ورودی‌های صفحه‌بندی.</param>
    /// <param name="searchTerm">عبارت جست‌وجوی متنی آزاد اختیاری که روی نام و ایمیل تطبیق داده می‌شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>صفحه‌ای از DTOهای مشتری.</returns>
    Task<PagedResult<CustomerDto>> GetPagedAsync(
        PaginationParams parameters,
        string? searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// مشتری موجود را با حفظ یکتایی آدرس ایمیل به‌روزرسانی می‌کند.
    /// </summary>
    /// <param name="id">شناسه‌ی مشتری.</param>
    /// <param name="request">بدنه‌ی درخواست به‌روزرسانی مشتری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>مشتری به‌روزرسانی‌شده به‌صورت DTO.</returns>
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// پس از بررسی وجود مشتری، او را حذف می‌کند.
    /// </summary>
    /// <param name="id">شناسه‌ی مشتری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی مشتری وجود نداشته باشد پرتاب می‌شود.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
