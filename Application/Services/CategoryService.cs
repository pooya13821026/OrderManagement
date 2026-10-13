using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Categories;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mapping;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Services;

/// <summary>
/// پیاده‌سازی مدیریت دسته‌بندی‌های محصول: ایجاد با یکتایی نام، بازیابی صفحه‌بندی‌شده،
/// به‌روزرسانی و حذف. همه‌ی ذخیره‌سازی از طریق <see cref="IUnitOfWork"/> انجام می‌شود.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="CategoryService"/> می‌سازد.
    /// </summary>
    /// <param name="unitOfWork">واحد کاری که برای همه‌ی ذخیره‌سازی‌ها استفاده می‌شود.</param>
    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.Name, nameof(request.Name));

        string normalizedName = request.Name.Trim();

        // نگهبان تکراری به‌صورت best-effort؛ ایندکس یکتای دیتابیس همچنان حافظ
        // مقتدرانه در برابر درج‌های همزمان است.
        if (await _unitOfWork.Categories.ExistsByNameAsync(normalizedName, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Category), $"دسته‌ای با نام '{normalizedName}' از قبل وجود دارد.");
        }

        DateTime now = DateTime.UtcNow;
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    /// <inheritdoc />
    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));

        Category category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Category), id);

        return category.ToDto();
    }

    /// <inheritdoc />
    public async Task<PagedResult<CategoryDto>> GetPagedAsync(
        PaginationParams parameters,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Normalize();

        PagedEntities<Category> page = await _unitOfWork.Categories.GetPagedAsync(
            string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim(),
            isActive,
            parameters.PageNumber,
            parameters.PageSize,
            cancellationToken);

        IReadOnlyList<CategoryDto> items = page.Items.Select(category => category.ToDto()).ToList();
        return new PagedResult<CategoryDto>(items, parameters.PageNumber, parameters.PageSize, page.TotalCount);
    }

    /// <inheritdoc />
    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.Name, nameof(request.Name));

        Category category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Category), id);

        string normalizedName = request.Name.Trim();
        if (!string.Equals(category.Name, normalizedName, StringComparison.OrdinalIgnoreCase)
            && await _unitOfWork.Categories.ExistsByNameAsync(normalizedName, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Category), $"دسته‌ای با نام '{normalizedName}' از قبل وجود دارد.");
        }

        category.Name = normalizedName;
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));

        Category category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Category), id);

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
