using OrderManagement.Application.Dtos.Auth;
using OrderManagement.Application.Dtos.Categories;
using OrderManagement.Application.Dtos.Customers;
using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Application.Dtos.Products;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Mapping;

/// <summary>
/// متدهای کمکی نگاشت استاتیک که موجودیت‌های دامنه را به نمای عمومی DTO آن‌ها تبدیل می‌کنند.
/// نگاشت در یک جا نگه‌داشته می‌شود تا از کپی‌های تکراری فیلدبه‌فیلد در سرویس‌ها جلوگیری شود.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// موجودیت <see cref="Customer"/> را به <see cref="CustomerDto"/> نگاشت می‌کند.
    /// </summary>
    /// <param name="customer">موجودیت مشتری‌ای که باید نگاشت شود.</param>
    /// <returns>DTO مشتری.</returns>
    public static CustomerDto ToDto(this Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    /// <summary>
    /// موجودیت <see cref="Category"/> را به <see cref="CategoryDto"/> نگاشت می‌کند.
    /// </summary>
    /// <param name="category">موجودیت دسته‌ای که باید نگاشت شود.</param>
    /// <returns>DTO دسته.</returns>
    public static CategoryDto ToDto(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    /// <summary>
    /// موجودیت <see cref="Product"/> را به <see cref="ProductDto"/> نگاشت می‌کند،
    /// شامل شناسه و نام دسته در صورت اتصال.
    /// </summary>
    /// <param name="product">موجودیت محصولی که باید نگاشت شود.</param>
    /// <returns>DTO محصول.</returns>
    public static ProductDto ToDto(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Sku = product.Sku,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    /// <summary>
    /// موجودیت <see cref="OrderItem"/> را با نام و SKU محصول مرتبط به <see cref="OrderItemDto"/> نگاشت می‌کند.
    /// محصولات ردیف‌های سفارش باید قبلاً بارگذاری شده باشند.
    /// </summary>
    /// <param name="orderItem">موجودیت ردیف سفارش که باید نگاشت شود.</param>
    /// <returns>DTO ردیف سفارش.</returns>
    public static OrderItemDto ToDto(this OrderItem orderItem)
    {
        ArgumentNullException.ThrowIfNull(orderItem);

        return new OrderItemDto
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            ProductName = orderItem.Product?.Name ?? string.Empty,
            Sku = orderItem.Product?.Sku ?? string.Empty,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice,
            LineTotal = orderItem.LineTotal
        };
    }

    /// <summary>
    /// موجودیت <see cref="OrderStatusHistory"/> را به <see cref="OrderStatusHistoryDto"/> نگاشت می‌کند.
    /// </summary>
    /// <param name="history">ردیف تاریخچه‌ی تغییر وضعیت که باید نگاشت شود.</param>
    /// <returns>DTO ردیف تاریخچه.</returns>
    public static OrderStatusHistoryDto ToDto(this OrderStatusHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);

        return new OrderStatusHistoryDto
        {
            OrderId = history.OrderId,
            FromStatus = history.FromStatus,
            ToStatus = history.ToStatus,
            ChangedAt = history.ChangedAt,
            ChangedByRole = history.ChangedByRole,
            Reason = history.Reason
        };
    }

    /// <summary>
    /// موجودیت <see cref="Order"/> را به <see cref="OrderDto"/> نگاشت می‌کند، شامل خلاصه‌ی مشتری،
    /// همه‌ی ردیف‌های سفارش، زمان‌نشان‌های وضعیت و تاریخچه‌ی گذارها (مرتب‌شده بر اساس زمان گذار).
    /// Customer و Items سفارش (با Product هر ردیف) و StatusHistory باید قبلاً بارگذاری شده باشند.
    /// </summary>
    /// <param name="order">موجودیت سفارشی که باید نگاشت شود.</param>
    /// <returns>DTO سفارش.</returns>
    public static OrderDto ToDto(this Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer is null
                ? string.Empty
                : $"{order.Customer.FirstName} {order.Customer.LastName}".Trim(),
            CustomerEmail = order.Customer?.Email ?? string.Empty,
            Status = order.Status,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            ConfirmedAt = order.ConfirmedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CancelledAt = order.CancelledAt,
            CancelReason = order.CancelReason,
            Items = order.Items?.Select(item => item.ToDto()).ToList() ?? new List<OrderItemDto>(),
            StatusHistory = order.StatusHistory?
                .OrderBy(history => history.ChangedAt)
                .Select(history => history.ToDto())
                .ToList() ?? new List<OrderStatusHistoryDto>()
        };
    }

    /// <summary>
    /// موجودیت <see cref="ApplicationUser"/> را به <see cref="UserDto"/> نگاشت می‌کند.
    /// هش گذرواژه و مهر امنیتی عمداً هرگز افشا نمی‌شوند.
    /// </summary>
    /// <param name="user">موجودیت کاربری که باید نگاشت شود.</param>
    /// <param name="customerId">شناسه‌ی پروفایل مشتری متصل به کاربر یا <c>null</c> اگر پیوندی نباشد؛
    /// فراخواننده (سرویس احراز هویت) آن را از مخزن مشتریان resolve می‌کند.</param>
    /// <returns>DTO کاربر.</returns>
    public static UserDto ToDto(this ApplicationUser user, Guid? customerId = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            CustomerId = customerId
        };
    }
}
