using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Products;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد سرویس برنامه برای مدیریت کاتالوگ محصولات.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// پس از بررسی اینکه کد SKU اشغال نشده باشد، محصول جدیدی ایجاد می‌کند.
    /// </summary>
    /// <param name="request">بدنه‌ی درخواست ایجاد محصول.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>محصول ایجادشده به‌صورت DTO.</returns>
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// محصول را بر اساس شناسه برمی‌گرداند.
    /// </summary>
    /// <param name="id">شناسه‌ی محصول.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>محصول به‌صورت DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی محصول وجود نداشته باشد پرتاب می‌شود.</exception>
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای فیلترشده و صفحه‌بندی‌شده از محصولات برمی‌گرداند.
    /// </summary>
    /// <param name="filter">ورودی‌های جست‌وجو، پرچم فعال بودن و صفحه‌بندی.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>صفحه‌ای از DTOهای محصول.</returns>
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductFilterParams filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// محصول موجود را با حفظ یکتایی SKU به‌روزرسانی می‌کند. موجودی اینجا تغییر نمی‌کند؛
    /// برای جابه‌جایی موجودی از <see cref="AdjustStockAsync"/> استفاده کنید.
    /// </summary>
    /// <param name="id">شناسه‌ی محصول.</param>
    /// <param name="request">بدنه‌ی درخواست به‌روزرسانی محصول.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>محصول به‌روزرسانی‌شده به‌صورت DTO.</returns>
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// پس از بررسی وجود محصول، آن را حذف می‌کند.
    /// </summary>
    /// <param name="id">شناسه‌ی محصول.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی محصول وجود نداشته باشد پرتاب می‌شود.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// موجودی محصول را با تغییر مقدار داده‌شده تنظیم می‌کند (مثبت برای افزایش،
    /// منفی برای کاهش). مقدار نهایی نباید منفی شود.
    /// </summary>
    /// <param name="productId">شناسه‌ی محصول.</param>
    /// <param name="quantityChange">تغییر علامت‌دار اعمال‌شده بر موجودی.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>محصول به‌روزرسانی‌شده به‌صورت DTO.</returns>
    Task<ProductDto> AdjustStockAsync(Guid productId, int quantityChange, CancellationToken cancellationToken = default);
}
