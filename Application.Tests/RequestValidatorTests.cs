using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Exceptions;
using Xunit;

namespace OrderManagement.Application.Tests;

/// <summary>
/// تست‌های واحد اعتبارسنج‌های دست‌ساز <see cref="RequestValidator"/>؛
/// بدون شبیه‌سازی و بدون پایگاه داده.
/// </summary>
public class RequestValidatorTests
{
    /// <summary>
    /// درخواست گروهی با یک عنصر <c>null</c> در فهرست سفارش‌ها باید DomainException پرتاب کند.
    /// </summary>
    [Fact]
    public void ValidateBulkOrderRequest_NullOrderElement_ThrowsDomain()
    {
        var request = new BulkCreateOrdersRequest
        {
            Orders = new List<CreateOrderRequest>
            {
                ValidOrder(),
                null!
            }
        };

        DomainException exception = Assert.Throws<DomainException>(
            () => RequestValidator.ValidateBulkOrderRequest(request));

        Assert.Equal("سفارش‌های درخواست گروهی نباید null باشند.", exception.Message);
    }

    /// <summary>
    /// درخواست سفارش با یک عنصر <c>null</c> در فهرست ردیف‌ها باید DomainException پرتاب کند.
    /// </summary>
    [Fact]
    public void ValidateOrderRequest_NullItemElement_ThrowsDomain()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                null!
            }
        };

        DomainException exception = Assert.Throws<DomainException>(
            () => RequestValidator.ValidateOrderRequest(request));

        Assert.Equal("ردیف‌های سفارش نباید null باشند.", exception.Message);
    }

    /// <summary>
    /// یک سفارش معتبر برای تست‌ها می‌سازد.
    /// </summary>
    private static CreateOrderRequest ValidOrder()
    {
        return new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest> { ValidItem() }
        };
    }

    /// <summary>
    /// یک ردیف معتبر برای تست‌ها می‌سازد.
    /// </summary>
    private static CreateOrderItemRequest ValidItem()
    {
        return new CreateOrderItemRequest
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1
        };
    }
}
