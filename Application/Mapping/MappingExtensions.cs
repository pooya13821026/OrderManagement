using OrderManagement.Application.Dtos.Auth;
using OrderManagement.Application.Dtos.Customers;
using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Application.Dtos.Products;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Mapping;

/// <summary>
/// Static mapping helpers converting domain entities to their public DTO projections.
/// Keeping the mapping in one place avoids duplicated field-by-field copies in services.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps a <see cref="Customer"/> entity to a <see cref="CustomerDto"/>.
    /// </summary>
    /// <param name="customer">The customer entity to map.</param>
    /// <returns>The customer DTO.</returns>
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
    /// Maps a <see cref="Product"/> entity to a <see cref="ProductDto"/>.
    /// </summary>
    /// <param name="product">The product entity to map.</param>
    /// <returns>The product DTO.</returns>
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
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    /// <summary>
    /// Maps an <see cref="OrderItem"/> entity to an <see cref="OrderItemDto"/> using the
    /// related product's name and SKU. The order must have its items' products loaded.
    /// </summary>
    /// <param name="orderItem">The order item entity to map.</param>
    /// <returns>The order item DTO.</returns>
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
    /// Maps an <see cref="Order"/> entity to an <see cref="OrderDto"/>, including the
    /// customer summary and all line items. The order must have Customer and Items
    /// (with each item's Product) loaded.
    /// </summary>
    /// <param name="order">The order entity to map.</param>
    /// <returns>The order DTO.</returns>
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
            Items = order.Items?.Select(item => item.ToDto()).ToList() ?? new List<OrderItemDto>()
        };
    }

    /// <summary>
    /// Maps an <see cref="ApplicationUser"/> entity to a <see cref="UserDto"/>.
    /// The password hash and security stamp are deliberately never exposed.
    /// </summary>
    /// <param name="user">The user entity to map.</param>
    /// <returns>The user DTO.</returns>
    public static UserDto ToDto(this ApplicationUser user)
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
            LastLoginAt = user.LastLoginAt
        };
    }
}
