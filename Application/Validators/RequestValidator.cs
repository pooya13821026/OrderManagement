using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Application.Validators;

/// <summary>
/// اعتبارسنج‌های دست‌ساز درخواست که توسط سرویس‌های برنامه استفاده می‌شوند.
/// DataAnnotations روی DTOها اعتبارسنجی سطح انتقال را پوشش می‌دهند؛ این بررسی‌ها همان
/// ثابت‌ها را وقتی DTOها در کد ساخته می‌شوند (برای نمونه توسط job های پس‌زمینه) محافظت
/// می‌کنند و قواعد کسب‌وکاری را اضافه می‌کنند که DataAnnotations نمی‌تواند بیان کند.
/// </summary>
public static class RequestValidator
{
    /// <summary>
    /// تضمین می‌کند که شناسه‌ی داده‌شده <see cref="Guid.Empty"/> نباشد.
    /// </summary>
    /// <param name="id">شناسه‌ای که باید بررسی شود.</param>
    /// <param name="parameterName">نام پارامتری که در پیام استثنا استفاده می‌شود.</param>
    /// <exception cref="DomainException">وقتی شناسه خالی باشد پرتاب می‌شود.</exception>
    public static void EnsureNotEmptyId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException($"{parameterName} نباید شناسه‌ی خالی باشد.");
        }
    }

    /// <summary>
    /// تضمین می‌کند که متن داده‌شده null، خالی یا فقط فاصله نباشد.
    /// </summary>
    /// <param name="value">متنی که باید بررسی شود.</param>
    /// <param name="parameterName">نام پارامتری که در پیام استثنا استفاده می‌شود.</param>
    /// <exception cref="DomainException">وقتی متن null، خالی یا فقط فاصله باشد پرتاب می‌شود.</exception>
    public static void EnsureNotEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} نباید null یا خالی باشد.");
        }
    }

    /// <summary>
    /// یک درخواست ایجاد سفارش را اعتبارسنجی می‌کند: شناسه‌ی مشتری، حداقل یک ردیف،
    /// شناسه‌های غیرخالی محصول و تعدادهای مثبت.
    /// </summary>
    /// <param name="request">درخواست سفارشی که باید اعتبارسنجی شود.</param>
    /// <exception cref="DomainException">وقتی درخواست نامعتبر باشد پرتاب می‌شود.</exception>
    public static void ValidateOrderRequest(CreateOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        EnsureNotEmptyId(request.CustomerId, nameof(request.CustomerId));

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new DomainException("یک سفارش باید حداقل یک ردیف داشته باشد.");
        }

        foreach (CreateOrderItemRequest item in request.Items)
        {
            if (item is null)
            {
                throw new DomainException("ردیف‌های سفارش نباید null باشند.");
            }

            EnsureNotEmptyId(item.ProductId, nameof(item.ProductId));

            if (item.Quantity <= 0)
            {
                throw new DomainException($"تعداد ردیف سفارش باید بزرگ‌تر از صفر باشد (محصول '{item.ProductId}').");
            }
        }
    }

    /// <summary>
    /// یک درخواست سفارش گروهی را اعتبارسنجی می‌کند: فهرست موجود، غیرخالی و حداکثر
    /// <see cref="BulkCreateOrdersRequest.MaxOrders"/> مورد؛ هر مورد با
    /// <see cref="ValidateOrderRequest"/> اعتبارسنجی می‌شود.
    /// </summary>
    /// <param name="request">درخواست گروهی‌ای که باید اعتبارسنجی شود.</param>
    /// <exception cref="DomainException">وقتی درخواست نامعتبر باشد پرتاب می‌شود.</exception>
    public static void ValidateBulkOrderRequest(BulkCreateOrdersRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Orders is null || request.Orders.Count == 0)
        {
            throw new DomainException("یک درخواست گروهی باید حداقل یک سفارش داشته باشد.");
        }

        if (request.Orders.Count > BulkCreateOrdersRequest.MaxOrders)
        {
            throw new DomainException(
                $"یک درخواست گروهی حداکثر می‌تواند {BulkCreateOrdersRequest.MaxOrders} سفارش داشته باشد؛ {request.Orders.Count} مورد ارسال شده است.");
        }

        foreach (CreateOrderRequest orderRequest in request.Orders)
        {
            ValidateOrderRequest(orderRequest);
        }
    }
}
