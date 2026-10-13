using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Customers;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mapping;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Services;

/// <summary>
/// پیاده‌سازی مدیریت مشتریان: ایجاد با یکتایی ایمیل، بازیابی صفحه‌بندی‌شده،
/// به‌روزرسانی و حذف. همه‌ی ذخیره‌سازی از طریق <see cref="IUnitOfWork"/> انجام می‌شود.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="CustomerService"/> می‌سازد.
    /// </summary>
    /// <param name="unitOfWork">واحد کاری که برای همه‌ی ذخیره‌سازی‌ها استفاده می‌شود.</param>
    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.Email, nameof(request.Email));

        string normalizedEmail = request.Email.Trim();

        // نگهبان تکراری به‌صورت best-effort؛ ایندکس یکتای دیتابیس همچنان حافظ
        // مقتدرانه در برابر درج‌های همزمان است.
        if (await _unitOfWork.Customers.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Customer), $"مشتری‌ای با ایمیل '{normalizedEmail}' از قبل وجود دارد.");
        }

        DateTime now = DateTime.UtcNow;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToDto();
    }

    /// <inheritdoc />
    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));

        Customer customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Customer), id);

        return customer.ToDto();
    }

    /// <inheritdoc />
    public async Task<PagedResult<CustomerDto>> GetPagedAsync(
        PaginationParams parameters,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Normalize();

        PagedEntities<Customer> page = await _unitOfWork.Customers.GetPagedAsync(
            string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim(),
            parameters.PageNumber,
            parameters.PageSize,
            cancellationToken);

        IReadOnlyList<CustomerDto> items = page.Items.Select(customer => customer.ToDto()).ToList();
        return new PagedResult<CustomerDto>(items, parameters.PageNumber, parameters.PageSize, page.TotalCount);
    }

    /// <inheritdoc />
    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.Email, nameof(request.Email));

        Customer customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Customer), id);

        string normalizedEmail = request.Email.Trim();
        if (!string.Equals(customer.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)
            && await _unitOfWork.Customers.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Customer), $"مشتری‌ای با ایمیل '{normalizedEmail}' از قبل وجود دارد.");
        }

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = normalizedEmail;
        customer.Phone = request.Phone?.Trim();
        customer.Address = request.Address?.Trim();
        customer.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToDto();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));

        Customer customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Customer), id);

        _unitOfWork.Customers.Remove(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
