using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Categories;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد سرویس برنامه برای مدیریت دسته‌بندی‌های محصول.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// پس از بررسی اینکه نام اشغال نشده باشد، دسته جدیدی ایجاد می‌کند.
    /// </summary>
    /// <param name="request">بدنه‌ی درخواست ایجاد دسته.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>دسته ایجادشده به‌صورت DTO.</returns>
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// دسته را بر اساس شناسه برمی‌گرداند.
    /// </summary>
    /// <param name="id">شناسه‌ی دسته.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>دسته به‌صورت DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی دسته وجود نداشته باشد پرتاب می‌شود.</exception>
    Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای فیلترشده و صفحه‌بندی‌شده از دسته‌ها برمی‌گرداند.
    /// </summary>
    /// <param name="parameters">ورودی‌های صفحه‌بندی.</param>
    /// <param name="searchTerm">عبارت جست‌وجوی متنی آزاد اختیاری که روی نام و توضیحات تطبیق داده می‌شود.</param>
    /// <param name="isActive">فیلتر اختیاری پرچم فعال بودن؛ <c>null</c> هم فعال و هم غیرفعال را برمی‌گرداند.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>صفحه‌ای از DTOهای دسته.</returns>
    Task<PagedResult<CategoryDto>> GetPagedAsync(
        PaginationParams parameters,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// دسته موجود را با حفظ یکتایی نام به‌روزرسانی می‌کند.
    /// </summary>
    /// <param name="id">شناسه‌ی دسته.</param>
    /// <param name="request">بدنه‌ی درخواست به‌روزرسانی دسته.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>دسته به‌روزرسانی‌شده به‌صورت DTO.</returns>
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// پس از بررسی وجود دسته، آن را حذف می‌کند. دسته‌ای که محصول به آن متصل است
    /// به‌خاطر محدودیت کلید خارجی قابل حذف نیست.
    /// </summary>
    /// <param name="id">شناسه‌ی دسته.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی دسته وجود نداشته باشد پرتاب می‌شود.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
