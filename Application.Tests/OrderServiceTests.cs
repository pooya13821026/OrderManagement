using Moq;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Repositories;
using Xunit;

namespace OrderManagement.Application.Tests;

/// <summary>
/// تست‌های واحد <see cref="OrderService"/> با شبیه‌سازی واحد کاری و مخزن‌ها؛
/// بدون EF Core، بدون پایگاه داده و بدون شبکه.
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IOrderItemRepository> _orderItemRepository = new();
    private readonly Mock<IRepository<OrderStatusHistory, Guid>> _orderStatusHistoryRepository = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly OrderService _service;

    /// <summary>
    /// نمونه‌ای جدید می‌سازد و مخزن‌های واحد کاری را به مخزن‌های شبیه‌سازی‌شده وصل می‌کند.
    /// </summary>
    public OrderServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Orders).Returns(_orderRepository.Object);
        _unitOfWork.SetupGet(u => u.Customers).Returns(_customerRepository.Object);
        _unitOfWork.SetupGet(u => u.Products).Returns(_productRepository.Object);
        _unitOfWork.SetupGet(u => u.OrderItems).Returns(_orderItemRepository.Object);
        _unitOfWork.SetupGet(u => u.OrderStatusHistory).Returns(_orderStatusHistoryRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new OrderService(_unitOfWork.Object, _notificationService.Object);
    }

    /// <summary>
    /// ایجاد سفارش معتبر باید سفارشی با وضعیت Pending، مبلغ کل درست و افزودن به مخزن و ذخیره‌سازی برگرداند.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsPendingOrder()
    {
        Customer customer = CreateCustomer();
        Product product = CreateProduct(stockQuantity: 10, price: 100m);
        _customerRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _orderRepository.Setup(r => r.ExistsByOrderNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = product.Id, Quantity = 3 }
            }
        };

        OrderDto result = await _service.CreateAsync(request, Guid.NewGuid(), Roles.Admin);

        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Single(result.Items);
        Assert.Equal(300m, result.TotalAmount);
        Assert.StartsWith("ORD-", result.OrderNumber);
        _orderRepository.Verify(
            r => r.AddAsync(
                It.Is<Order>(o => o.Status == OrderStatus.Pending && o.CustomerId == customer.Id && o.TotalAmount == 300m),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ایجاد سفارش برای مشتری ناشناخته باید EntityNotFoundException پرتاب کند و سفارشی ذخیره نشود.
    /// </summary>
    [Fact]
    public async Task CreateAsync_UnknownCustomer_ThrowsEntityNotFound()
    {
        Guid customerId = Guid.NewGuid();
        _customerRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.CreateAsync(request, Guid.NewGuid(), Roles.Admin));

        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ایجاد سفارش با محصول غیرفعال باید DomainException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task CreateAsync_InactiveProduct_ThrowsDomain()
    {
        Customer customer = CreateCustomer();
        Product product = CreateProduct(stockQuantity: 5, price: 10m, isActive: false);
        _customerRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = product.Id, Quantity = 1 }
            }
        };

        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(request, Guid.NewGuid(), Roles.Admin));

        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// تأیید سفارش باید وضعیت را Confirmed کند و موجودی محصولات ردیف‌ها را دقیقاً به اندازه‌ی تعداد کم کند.
    /// </summary>
    [Fact]
    public async Task UpdateStatusAsync_PendingToConfirmed_DecrementsStock()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 2);
        Guid ownerId = Guid.NewGuid();
        order.Customer.UserId = ownerId;
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        OrderDto result = await _service.UpdateStatusAsync(order.Id, OrderStatus.Confirmed, Roles.User, changedByUserId: ownerId);

        Assert.Equal(OrderStatus.Confirmed, result.Status);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(8, product.StockQuantity);
        _productRepository.Verify(r => r.Update(product), Times.Once);
        _orderRepository.Verify(r => r.Update(order), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// تأیید سفارش با موجودی ناکافی باید InsufficientStockException پرتاب کند و موجودی دست‌نخورده بماند (بدون کاهش جزئی).
    /// </summary>
    [Fact]
    public async Task UpdateStatusAsync_ConfirmWithShortStock_ThrowsInsufficientStock()
    {
        Product product = CreateProduct(stockQuantity: 1, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 2);
        Guid ownerId = Guid.NewGuid();
        order.Customer.UserId = ownerId;
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        InsufficientStockException exception = await Assert.ThrowsAsync<InsufficientStockException>(
            () => _service.UpdateStatusAsync(order.Id, OrderStatus.Confirmed, Roles.User, changedByUserId: ownerId));

        Assert.Equal(product.Id, exception.ProductId);
        Assert.Equal(1, exception.Available);
        Assert.Equal(2, exception.Requested);
        Assert.Equal(1, product.StockQuantity);
        Assert.Equal(OrderStatus.Pending, order.Status);
        _productRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepository.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// تأیید سفارش با دو ردیف از یک محصول (۳+۳) در برابر موجودی ۵ باید InsufficientStockException بدهد؛
    /// تعداد درخواستیِ ردیف‌های هم‌محصول جمع می‌شود و موجودی دست‌نخورده می‌ماند (بدون کاهش جزئی).
    /// </summary>
    [Fact]
    public async Task UpdateStatusAsync_ConfirmWithDuplicateProductLines_ThrowsInsufficientStock()
    {
        Product product = CreateProduct(stockQuantity: 5, price: 50m);
        Order order = CreateOrderWithTwoLinesOfSameProduct(product, firstQuantity: 3, secondQuantity: 3);
        Guid ownerId = Guid.NewGuid();
        order.Customer.UserId = ownerId;
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        InsufficientStockException exception = await Assert.ThrowsAsync<InsufficientStockException>(
            () => _service.UpdateStatusAsync(order.Id, OrderStatus.Confirmed, Roles.User, changedByUserId: ownerId));

        Assert.Equal(product.Id, exception.ProductId);
        Assert.Equal(5, exception.Available);
        Assert.Equal(6, exception.Requested);
        Assert.Equal(5, product.StockQuantity);
        Assert.Equal(OrderStatus.Pending, order.Status);
        _productRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepository.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// پرش از یک مرحله (Pending به Shipped) باید InvalidStatusTransitionException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task UpdateStatusAsync_SkipStep_ThrowsInvalidTransition()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1);
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => _service.UpdateStatusAsync(order.Id, OrderStatus.Shipped, Roles.Admin));

        Assert.Equal(OrderStatus.Pending, order.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// لغو سفارش توسط کاربر عادی باید UnauthorizedOperationException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task UpdateStatusAsync_CancelByUser_ThrowsUnauthorized()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1);
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        UnauthorizedOperationException exception = await Assert.ThrowsAsync<UnauthorizedOperationException>(
            () => _service.UpdateStatusAsync(order.Id, OrderStatus.Cancelled, Roles.User));

        Assert.Equal("فقط مدیران اجازه‌ی لغو سفارش‌ها را دارند.", exception.Message);
        Assert.Equal(OrderStatus.Pending, order.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// لغو سفارش توسط مدیر باید موفق باشد و وضعیت Cancelled شود.
    /// </summary>
    [Fact]
    public async Task UpdateStatusAsync_CancelByAdmin_Succeeds()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1);
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        OrderDto result = await _service.UpdateStatusAsync(order.Id, OrderStatus.Cancelled, Roles.Admin);

        Assert.Equal(OrderStatus.Cancelled, result.Status);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(10, product.StockQuantity);
        _orderRepository.Verify(r => r.Update(order), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// لغو سفارش تأییدشده باید دقیقاً به اندازه‌ای که Confirm موجودی کم کرده بود برگرداند.
    /// </summary>
    [Fact]
    public async Task CancelFromConfirmed_RestoresStock()
    {
        Product product = CreateProduct(stockQuantity: 8, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 2, status: OrderStatus.Confirmed);
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        OrderDto result = await _service.UpdateStatusAsync(order.Id, OrderStatus.Cancelled, Roles.Admin);

        Assert.Equal(OrderStatus.Cancelled, result.Status);
        Assert.Equal(10, product.StockQuantity);
        _productRepository.Verify(r => r.Update(product), Times.Once);
        _orderRepository.Verify(r => r.Update(order), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// لغو سفارش Pending هرگز موجودی را دست نمی‌زند چون Confirm رخ نداده و چیزی برای بازگرداندن نیست.
    /// </summary>
    [Fact]
    public async Task CancelFromPending_DoesNotTouchStock()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 2, status: OrderStatus.Pending);
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        OrderDto result = await _service.UpdateStatusAsync(order.Id, OrderStatus.Cancelled, Roles.Admin);

        Assert.Equal(OrderStatus.Cancelled, result.Status);
        Assert.Equal(10, product.StockQuantity);
        _productRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// هر گذار موفق باید دقیقاً یک ردیف تاریخچه با وضعیت مبدأ/مقصد، زمان، نقش و شناسه‌ی
    /// کاربر ثبت کند و پس از commit اطلاع‌رسانی (لاگ) فراخوانی شود.
    /// </summary>
    [Fact]
    public async Task StatusChange_RecordsHistoryRow()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1);
        Guid changedByUserId = Guid.NewGuid();
        order.Customer.UserId = changedByUserId;
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await _service.UpdateStatusAsync(
            order.Id,
            OrderStatus.Confirmed,
            Roles.User,
            reason: null,
            changedByUserId: changedByUserId);

        _orderStatusHistoryRepository.Verify(
            r => r.AddAsync(
                It.Is<OrderStatusHistory>(h =>
                    h.OrderId == order.Id
                    && h.FromStatus == OrderStatus.Pending
                    && h.ToStatus == OrderStatus.Confirmed
                    && h.ChangedByUserId == changedByUserId
                    && h.ChangedByRole == Roles.User
                    && h.ChangedAt != default
                    && h.Reason == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationService.Verify(
            n => n.NotifyOrderStatusChangedAsync(
                order.Id,
                order.OrderNumber,
                OrderStatus.Pending,
                OrderStatus.Confirmed,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// هر وضعیت جدید باید زمان‌نشان مخصوص خود را ثبت کند و بقیه‌ی زمان‌نشان‌ها <c>null</c> بمانند.
    /// </summary>
    [Fact]
    public async Task StatusChange_SetsStateTimestamp()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1);
        Guid ownerId = Guid.NewGuid();
        order.Customer.UserId = ownerId;
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await _service.UpdateStatusAsync(order.Id, OrderStatus.Confirmed, Roles.User, changedByUserId: ownerId);

        Assert.NotNull(order.ConfirmedAt);
        Assert.Null(order.ShippedAt);
        Assert.Null(order.DeliveredAt);
        Assert.Null(order.CancelledAt);

        await _service.UpdateStatusAsync(order.Id, OrderStatus.Shipped, Roles.User, changedByUserId: ownerId);

        Assert.NotNull(order.ShippedAt);
        Assert.NotNull(order.ConfirmedAt);
        Assert.Null(order.CancelledAt);
    }

    /// <summary>
    /// دلیل لغو باید trim شده در <c>CancelReason</c> سفارش و ردیف تاریخچه ثبت شود.
    /// </summary>
    [Fact]
    public async Task Cancel_SavesReason()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1, status: OrderStatus.Confirmed);
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await _service.UpdateStatusAsync(
            order.Id,
            OrderStatus.Cancelled,
            Roles.Admin,
            reason: "  درخواست مشتری  ");

        Assert.Equal("درخواست مشتری", order.CancelReason);
        Assert.NotNull(order.CancelledAt);
        _orderStatusHistoryRepository.Verify(
            r => r.AddAsync(
                It.Is<OrderStatusHistory>(h => h.Reason == "درخواست مشتری" && h.ToStatus == OrderStatus.Cancelled),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// ماتریس کامل گذارهای وضعیت: فقط رو به جلو و تک‌مرحله‌ای به‌علاوه‌ی Cancelled از Pending یا Confirmed مجاز است.
    /// </summary>
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Cancelled, false)]
    public void CanTransition_Matrix(OrderStatus currentStatus, OrderStatus requestedStatus, bool expected)
    {
        Assert.Equal(expected, _service.CanTransition(currentStatus, requestedStatus));
    }

    /// <summary>
    /// ایجاد گروهی با دسته‌ی مخلوط (یک سفارش معتبر و یک سفارش با مشتری ناشناخته) باید نتیجه‌ی هر ردیف را جداگانه گزارش کند.
    /// </summary>
    [Fact]
    public async Task BulkCreateAsync_MixedBatch_ReturnsPerItemResults()
    {
        Customer customer = CreateCustomer();
        Product product = CreateProduct(stockQuantity: 10, price: 100m);
        _customerRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _orderRepository.Setup(r => r.ExistsByOrderNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new BulkCreateOrdersRequest
        {
            Orders = new List<CreateOrderRequest>
            {
                new()
                {
                    CustomerId = customer.Id,
                    Items = new List<CreateOrderItemRequest> { new() { ProductId = product.Id, Quantity = 1 } }
                },
                new()
                {
                    CustomerId = Guid.NewGuid(),
                    Items = new List<CreateOrderItemRequest> { new() { ProductId = product.Id, Quantity = 1 } }
                }
            }
        };

        BulkCreateOrdersResponse response = await _service.BulkCreateAsync(request);

        Assert.Equal(1, response.SuccessfulCount);
        Assert.Equal(1, response.FailedCount);
        Assert.Equal(2, response.Results.Count);

        BulkOrderResultItem success = response.Results[0];
        Assert.Equal(0, success.Index);
        Assert.True(success.Success);
        Assert.True(success.OrderId.HasValue);
        Assert.False(string.IsNullOrEmpty(success.OrderNumber));
        Assert.Null(success.ErrorMessage);

        BulkOrderResultItem failure = response.Results[1];
        Assert.Equal(1, failure.Index);
        Assert.False(failure.Success);
        Assert.False(failure.OrderId.HasValue);
        Assert.False(string.IsNullOrEmpty(failure.ErrorMessage));

        _orderRepository.Verify(
            r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// حذف سفارش ناشناخته باید EntityNotFoundException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_UnknownOrder_ThrowsEntityNotFound()
    {
        Guid orderId = Guid.NewGuid();
        _orderRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(orderId));

        _orderRepository.Verify(r => r.Remove(It.IsAny<Order>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ایجاد سفارش توسط کاربر عادی با مشتریِ متصل به خودش باید موفق باشد.
    /// </summary>
    [Fact]
    public async Task CreateAsync_AsUserWithOwnCustomer_Succeeds()
    {
        Guid userId = Guid.NewGuid();
        Customer customer = CreateCustomer();
        customer.UserId = userId;
        Product product = CreateProduct(stockQuantity: 10, price: 100m);
        _customerRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _orderRepository.Setup(r => r.ExistsByOrderNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = new List<CreateOrderItemRequest> { new() { ProductId = product.Id, Quantity = 1 } }
        };

        OrderDto result = await _service.CreateAsync(request, userId, Roles.User);

        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(OrderStatus.Pending, result.Status);
    }

    /// <summary>
    /// ایجاد سفارش توسط کاربر عادی با مشتریِ دیگری باید UnauthorizedOperationException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task CreateAsync_AsUserWithForeignCustomerId_ThrowsUnauthorized()
    {
        Guid userId = Guid.NewGuid();
        Customer ownCustomer = CreateCustomer();
        ownCustomer.UserId = userId;
        _customerRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(ownCustomer);

        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
        };

        await Assert.ThrowsAsync<UnauthorizedOperationException>(
            () => _service.CreateAsync(request, userId, Roles.User));

        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ایجاد سفارش توسط کاربر عادیِ بدون پروفایل مشتری باید DomainException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task CreateAsync_AsUserWithoutProfile_ThrowsDomain()
    {
        Guid userId = Guid.NewGuid();
        _customerRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
        };

        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(request, userId, Roles.User));

        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// دیدن سفارش دیگری توسط کاربر عادی باید UnauthorizedOperationException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_AsNonOwner_ThrowsUnauthorized()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1);
        order.Customer.UserId = Guid.NewGuid();
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<UnauthorizedOperationException>(
            () => _service.GetByIdAsync(order.Id, Guid.NewGuid(), Roles.User));
    }

    /// <summary>
    /// مدیر باید سفارش کاربران دیگر را هم ببیند.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_AsAdminSeesOthersOrder_Returns()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1);
        order.Customer.UserId = Guid.NewGuid();
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        OrderDto result = await _service.GetByIdAsync(order.Id, Guid.NewGuid(), Roles.Admin);

        Assert.Equal(order.Id, result.Id);
    }

    /// <summary>
    /// فهرست سفارش‌های کاربر عادی باید همیشه با مشتریِ خودش فیلتر شود، حتی اگر customerId دیگری بفرستد.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_AsUser_ForcesOwnCustomer()
    {
        Guid userId = Guid.NewGuid();
        Customer ownCustomer = CreateCustomer();
        ownCustomer.UserId = userId;
        _customerRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(ownCustomer);
        _orderRepository.Setup(r => r.GetPagedAsync(
                It.IsAny<Guid?>(),
                It.IsAny<OrderStatus?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedEntities<Order>(new List<Order>(), 0));

        var filter = new OrderFilterParams { CustomerId = Guid.NewGuid(), PageNumber = 1, PageSize = 10 };

        PagedResult<OrderDto> result = await _service.GetPagedAsync(filter, userId, Roles.User);

        Assert.Equal(0, result.TotalCount);
        _orderRepository.Verify(r => r.GetPagedAsync(
            It.Is<Guid?>(customerId => customerId == ownCustomer.Id),
            It.IsAny<OrderStatus?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// فهرست سفارش‌های کاربر عادیِ بدون پروفایل باید خالی برگردد، نه خطا.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_AsUserWithoutProfile_ReturnsEmpty()
    {
        Guid userId = Guid.NewGuid();
        _customerRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        PagedResult<OrderDto> result = await _service.GetPagedAsync(new OrderFilterParams(), userId, Roles.User);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
        _orderRepository.Verify(r => r.GetPagedAsync(
            It.IsAny<Guid?>(),
            It.IsAny<OrderStatus?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// جلو بردن سفارش دیگری توسط کاربر عادی باید UnauthorizedOperationException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task UpdateStatusAsync_AsNonOwner_ThrowsUnauthorized()
    {
        Product product = CreateProduct(stockQuantity: 10, price: 50m);
        Order order = CreateOrderWithItem(product, quantity: 1);
        order.Customer.UserId = Guid.NewGuid();
        _orderRepository.Setup(r => r.GetByIdWithDetailsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<UnauthorizedOperationException>(
            () => _service.UpdateStatusAsync(order.Id, OrderStatus.Confirmed, Roles.User, changedByUserId: Guid.NewGuid()));

        Assert.Equal(OrderStatus.Pending, order.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// یک مشتری ساده برای تست‌ها می‌سازد.
    /// </summary>
    private static Customer CreateCustomer()
    {
        DateTime now = DateTime.UtcNow;
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Ali",
            LastName = "Rezaei",
            Email = "ali@example.com",
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// یک محصول ساده برای تست‌ها می‌سازد.
    /// </summary>
    private static Product CreateProduct(int stockQuantity, decimal price, bool isActive = true)
    {
        DateTime now = DateTime.UtcNow;
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop",
            Sku = $"SKU-{Guid.NewGuid():N}",
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// یک سفارش با یک ردیف و ناوبری Product پر می‌سازد (مثل خروجی GetByIdWithDetailsAsync).
    /// </summary>
    private static Order CreateOrderWithItem(Product product, int quantity, OrderStatus status = OrderStatus.Pending)
    {
        DateTime now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            OrderNumber = $"ORD-{now:yyyyMMdd}-000001",
            Status = status,
            OrderDate = now,
            CreatedAt = now,
            UpdatedAt = now,
            Customer = CreateCustomer(),
            Items = new List<OrderItem>(),
            StatusHistory = new List<OrderStatusHistory>()
        };

        order.Items.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = product.Price,
            CreatedAt = now,
            Product = product
        });

        return order;
    }

    /// <summary>
    /// سفارشی با دو ردیف از یک نمونه‌ی واحد محصول می‌سازد (تداخل دو ردیف هم‌محصول).
    /// </summary>
    private static Order CreateOrderWithTwoLinesOfSameProduct(Product product, int firstQuantity, int secondQuantity)
    {
        Order order = CreateOrderWithItem(product, firstQuantity);
        order.Items.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = secondQuantity,
            UnitPrice = product.Price,
            CreatedAt = DateTime.UtcNow,
            Product = product
        });

        return order;
    }
}
