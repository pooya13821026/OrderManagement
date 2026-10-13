using System.Security.Cryptography;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mapping;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Services;

/// <summary>
/// پیاده‌سازی چرخه‌ی حیات سفارش: ایجاد با اسنپ‌شات قیمت‌ها و شماره‌های سفارش یکتا،
/// بازیابی صفحه‌بندی‌شده، گذارهای رو به جلو با اعتبارسنجی موجودی هنگام تأیید،
/// حذف و ایجاد گروهی با نتیجه‌ی هر سفارش. همه‌ی ذخیره‌سازی از طریق
/// <see cref="IUnitOfWork"/> انجام می‌شود؛ پیاده‌سازی‌های مشخص در Infrastructure قرار دارند.
/// </summary>
public class OrderService : IOrderService
{
    /// <summary>
    /// تعداد تلاش برای تولید شماره‌ی سفارش استفاده‌نشده پیش از تسلیم شدن.
    /// </summary>
    private const int OrderNumberGenerationAttempts = 5;

    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="OrderService"/> می‌سازد.
    /// </summary>
    /// <param name="unitOfWork">واحد کاری که برای همه‌ی ذخیره‌سازی‌ها استفاده می‌شود.</param>
    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.ValidateOrderRequest(request);

        Customer customer = await GetCustomerOrFailAsync(request.CustomerId, cancellationToken);
        IReadOnlyDictionary<Guid, Product> productsById = await LoadOrderedProductsOrFailAsync(request, cancellationToken);
        string orderNumber = await GenerateUniqueOrderNumberAsync(cancellationToken);
        Order order = BuildOrder(request, orderNumber, customer, productsById);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return order.ToDto();
    }

    /// <inheritdoc />
    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));

        Order order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Order), id);

        return order.ToDto();
    }

    /// <inheritdoc />
    public async Task<PagedResult<OrderDto>> GetPagedAsync(OrderFilterParams filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        filter.Normalize();

        PagedEntities<Order> page = await _unitOfWork.Orders.GetPagedAsync(
            filter.CustomerId,
            filter.Status,
            filter.FromDate,
            filter.ToDate,
            string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm.Trim(),
            string.IsNullOrWhiteSpace(filter.SortBy) ? null : filter.SortBy.Trim(),
            filter.SortDescending,
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        IReadOnlyList<OrderDto> items = page.Items.Select(order => order.ToDto()).ToList();
        return new PagedResult<OrderDto>(items, filter.PageNumber, filter.PageSize, page.TotalCount);
    }

    /// <inheritdoc />
    public async Task<OrderDto> UpdateStatusAsync(
        Guid orderId,
        OrderStatus newStatus,
        string requesterRole,
        CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(orderId, nameof(orderId));

        Order order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Order), orderId);

        if (newStatus == OrderStatus.Cancelled
            && !string.Equals(requesterRole, Roles.Admin, StringComparison.Ordinal))
        {
            throw new UnauthorizedOperationException("فقط مدیران اجازه‌ی لغو سفارش‌ها را دارند.");
        }

        if (!CanTransition(order.Status, newStatus))
        {
            throw new InvalidStatusTransitionException(order.Status, newStatus);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (newStatus == OrderStatus.Confirmed)
            {
                await ValidateAndDecrementStockAsync(order, cancellationToken);
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return order.ToDto();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));

        // فقط بررسی وجود؛ اعمال نقش‌ها توسط لایه‌ی API انجام می‌شود.
        Order order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Order), id);

        _unitOfWork.Orders.Remove(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BulkCreateOrdersResponse> BulkCreateAsync(
        BulkCreateOrdersRequest request,
        CancellationToken cancellationToken = default)
    {
        RequestValidator.ValidateBulkOrderRequest(request);

        // بارگذاری همه‌ی محصولات ارجاع‌داده‌شده یک بار؛ سپس برای هر سفارش فقط عضویت
        // و وضعیت فعال بودن بررسی می‌شود که ایجاد گروهی را روی دسته‌های بزرگ سریع نگه می‌دارد.
        IReadOnlyDictionary<Guid, Product> productsById = await LoadBulkProductsOrFailAsync(request, cancellationToken);
        var customersById = new Dictionary<Guid, Customer>();
        var results = new List<BulkOrderResultItem>(request.Orders.Count);
        int successfulCount = 0;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            for (int index = 0; index < request.Orders.Count; index++)
            {
                CreateOrderRequest orderRequest = request.Orders[index];
                try
                {
                    if (!customersById.TryGetValue(orderRequest.CustomerId, out Customer? customer))
                    {
                        customer = await GetCustomerOrFailAsync(orderRequest.CustomerId, cancellationToken);
                        customersById[orderRequest.CustomerId] = customer;
                    }

                    string orderNumber = await GenerateUniqueOrderNumberAsync(cancellationToken);
                    Order order = BuildOrder(orderRequest, orderNumber, customer, productsById);

                    await _unitOfWork.Orders.AddAsync(order, cancellationToken);
                    // ذخیره‌سازی per order داخل تراکنش مشترک تا هر سفارش ایجادشده
                    // شناسه‌اش را بگیرد و در عین حال کل دسته به‌صورت اتمیک commit شود.
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    results.Add(new BulkOrderResultItem
                    {
                        Index = index,
                        Success = true,
                        OrderId = order.Id,
                        OrderNumber = order.OrderNumber
                    });
                    successfulCount++;
                }
                catch (Exception exception)
                {
                    // پردازش سفارش‌های باقی‌مانده ادامه می‌یابد؛ شکست یک سفارش
                    // نباید نتایج سفارش‌های خواهر و برادرش را دور بریزد.
                    results.Add(new BulkOrderResultItem
                    {
                        Index = index,
                        Success = false,
                        ErrorMessage = exception.Message
                    });
                }
            }

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return new BulkCreateOrdersResponse
        {
            SuccessfulCount = successfulCount,
            FailedCount = request.Orders.Count - successfulCount,
            Results = results
        };
    }

    /// <inheritdoc />
    public bool CanTransition(OrderStatus currentStatus, OrderStatus requestedStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => requestedStatus is OrderStatus.Confirmed or OrderStatus.Cancelled,
            OrderStatus.Confirmed => requestedStatus is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => requestedStatus == OrderStatus.Delivered,
            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }

    /// <summary>
    /// موجودیت <see cref="Order"/> جدیدی را از درخواست می‌سازد: قیمت فعلی هر محصول را در
    /// ردیف‌های سفارش اسنپ‌شات می‌کند و قیمت کل ردیف‌ها و مبلغ کل سفارش را محاسبه می‌کند.
    /// قیمت کاتالوگ محصول ممکن است بعداً تغییر کند؛ سفارش‌ها همیشه اسنپ‌شات خود را نگه می‌دارند.
    /// </summary>
    private static Order BuildOrder(
        CreateOrderRequest request,
        string orderNumber,
        Customer customer,
        IReadOnlyDictionary<Guid, Product> productsById)
    {
        DateTime now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            OrderNumber = orderNumber,
            Status = OrderStatus.Pending,
            OrderDate = now,
            ShippingAddress = request.ShippingAddress?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
            Customer = customer
        };

        decimal totalAmount = 0;
        foreach (CreateOrderItemRequest itemRequest in request.Items)
        {
            Product product = productsById[itemRequest.ProductId];
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price,
                CreatedAt = now,
                Product = product
            };

            totalAmount += orderItem.LineTotal;
            order.Items.Add(orderItem);
        }

        order.TotalAmount = totalAmount;
        return order;
    }

    /// <summary>
    /// شماره‌ی سفارش را با قالب <c>ORD-yyyyMMdd-xxxxxx</c> و شش رقم تصادفی تولید می‌کند
    /// و تا زمانی که شماره قبلاً اشغال نشده باشد تلاش می‌کند.
    /// </summary>
    private async Task<string> GenerateUniqueOrderNumberAsync(CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < OrderNumberGenerationAttempts; attempt++)
        {
            string orderNumber = GenerateOrderNumber();
            if (!await _unitOfWork.Orders.ExistsByOrderNumberAsync(orderNumber, cancellationToken))
            {
                return orderNumber;
            }
        }

        throw new DomainException(
            $"پس از {OrderNumberGenerationAttempts} تلاش نتوانستم شماره‌ی سفارش یکتا تولید کنم.");
    }

    /// <summary>
    /// یک شماره‌ی سفارش با قالب <c>ORD-yyyyMMdd-xxxxxx</c> می‌سازد.
    /// ارقام تصادفی از مولد رمزنگاری می‌آیند، نه از <see cref="Random"/>.
    /// </summary>
    private static string GenerateOrderNumber()
    {
        string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        int suffix = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return $"ORD-{datePart}-{suffix:D6}";
    }

    /// <summary>
    /// مشتری سفارش را بارگذاری می‌کند یا در صورت نبودن پرتاب می‌کند.
    /// </summary>
    private async Task<Customer> GetCustomerOrFailAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Customer), customerId);
    }

    /// <summary>
    /// محصولات یکتای یک درخواست سفارش را بارگذاری می‌کند، تضمین می‌کند هر محصول وجود دارد
    /// و فعال است، و آن‌ها را با کلید شناسه‌ی محصول برمی‌گرداند.
    /// </summary>
    private async Task<IReadOnlyDictionary<Guid, Product>> LoadOrderedProductsOrFailAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        IEnumerable<Guid> productIds = request.Items.Select(item => item.ProductId).Distinct();
        IReadOnlyList<Product> products = await _unitOfWork.Products.GetByIdsAsync(productIds, cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);

        foreach (CreateOrderItemRequest item in request.Items)
        {
            if (!productsById.TryGetValue(item.ProductId, out Product? product))
            {
                throw new EntityNotFoundException(nameof(Product), item.ProductId);
            }

            if (!product.IsActive)
            {
                throw new DomainException($"محصول '{product.Id}' فعال نیست و نمی‌توان آن را سفارش داد.");
            }
        }

        return productsById;
    }

    /// <summary>
    /// محصولات یکتای یک درخواست گروهی را یک بار بارگذاری می‌کند، با همان بررسی‌های وجود و
    /// فعال بودن <see cref="LoadOrderedProductsOrFailAsync"/>.
    /// </summary>
    private async Task<IReadOnlyDictionary<Guid, Product>> LoadBulkProductsOrFailAsync(
        BulkCreateOrdersRequest request,
        CancellationToken cancellationToken)
    {
        IEnumerable<Guid> productIds = request.Orders
            .SelectMany(order => order.Items)
            .Select(item => item.ProductId)
            .Distinct();

        IReadOnlyList<Product> products = await _unitOfWork.Products.GetByIdsAsync(productIds, cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);

        foreach (CreateOrderRequest orderRequest in request.Orders)
        {
            foreach (CreateOrderItemRequest item in orderRequest.Items)
            {
                if (!productsById.TryGetValue(item.ProductId, out Product? product))
                {
                    throw new EntityNotFoundException(nameof(Product), item.ProductId);
                }

                if (!product.IsActive)
                {
                    throw new DomainException($"محصول '{product.Id}' فعال نیست و نمی‌توان آن را سفارش داد.");
                }
            }
        }

        return productsById;
    }

    /// <summary>
    /// ابتدا موجودی همه‌ی اقلام سفارش را اعتبارسنجی و سپس کاهش می‌دهد.
    /// بررسی همه‌چیز از پیش تضمین می‌کند که کمبود در ردیف بعدی هرگز
    /// ردیف‌های قبلی را از قبل کاهش‌داده‌شده رها نکند (بدون کاهش جزئی موجودی).
    /// </summary>
    private async Task ValidateAndDecrementStockAsync(Order order, CancellationToken cancellationToken)
    {
        foreach (OrderItem item in order.Items)
        {
            Product product = item.Product
                ?? throw new EntityNotFoundException(nameof(Product), item.ProductId);

            if (product.StockQuantity < item.Quantity)
            {
                throw new InsufficientStockException(product.Id, product.StockQuantity, item.Quantity);
            }
        }

        foreach (OrderItem item in order.Items)
        {
            item.Product.StockQuantity -= item.Quantity;
            item.Product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(item.Product);
        }
    }
}
