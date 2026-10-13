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
/// Implements the order lifecycle: creation with price snapshots and unique order numbers,
/// paged retrieval, forward-only status transitions with stock validation on confirmation,
/// deletion and bulk creation with per-order results. All persistence goes through
/// <see cref="IUnitOfWork"/>; the concrete implementations live in Infrastructure.
/// </summary>
public class OrderService : IOrderService
{
    /// <summary>
    /// Number of attempts to generate an unused order number before giving up.
    /// </summary>
    private const int OrderNumberGenerationAttempts = 5;

    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used for all persistence.</param>
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
            throw new UnauthorizedOperationException("Only administrators are allowed to cancel orders.");
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

        // Existence check only; role enforcement is done by the API layer.
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

        // Load every referenced product once; per-order we then only check membership
        // and active state, which keeps bulk creation fast on large batches.
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
                    // Save per order inside the shared transaction so each created order
                    // gets its identifier while the whole batch still commits atomically.
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
                    // Keep processing the remaining orders; the failure of one order
                    // must not discard the results of its siblings.
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
    /// Builds a new <see cref="Order"/> entity from the request: snapshots each product's
    /// current price into the line items and computes line totals and the order total.
    /// The product catalog price may change later; orders always keep their snapshot.
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
    /// Generates the order number in the format <c>ORD-yyyyMMdd-xxxxxx</c> with six random
    /// digits, retrying until the number is not already taken.
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
            $"Unable to generate a unique order number after {OrderNumberGenerationAttempts} attempts.");
    }

    /// <summary>
    /// Builds a single order number in the format <c>ORD-yyyyMMdd-xxxxxx</c>.
    /// Random digits come from the cryptographic generator, not <see cref="Random"/>.
    /// </summary>
    private static string GenerateOrderNumber()
    {
        string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        int suffix = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return $"ORD-{datePart}-{suffix:D6}";
    }

    /// <summary>
    /// Loads the customer for the order or throws when it does not exist.
    /// </summary>
    private async Task<Customer> GetCustomerOrFailAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Customer), customerId);
    }

    /// <summary>
    /// Loads the distinct products of a single order request, ensuring every product exists
    /// and is active, and returns them keyed by product identifier.
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
                throw new DomainException($"Product '{product.Id}' is not active and cannot be ordered.");
            }
        }

        return productsById;
    }

    /// <summary>
    /// Loads the distinct products of a bulk request once, with the same existence and
    /// active checks as <see cref="LoadOrderedProductsOrFailAsync"/>.
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
                    throw new DomainException($"Product '{product.Id}' is not active and cannot be ordered.");
                }
            }
        }

        return productsById;
    }

    /// <summary>
    /// Validates stock for every item of the order first and only then decrements it.
    /// Checking everything up front guarantees that a shortage on a later item never
    /// leaves earlier items already decremented (no partial stock reduction).
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
