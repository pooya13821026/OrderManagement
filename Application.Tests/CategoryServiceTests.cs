using Moq;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Categories;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Repositories;
using Xunit;

namespace OrderManagement.Application.Tests;

/// <summary>
/// تست‌های واحد <see cref="CategoryService"/> با شبیه‌سازی واحد کاری و مخزن دسته‌ها؛
/// بدون EF Core، بدون پایگاه داده و بدون شبکه.
/// </summary>
public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly CategoryService _service;

    /// <summary>
    /// نمونه‌ای جدید می‌سازد و مخزن دسته‌های واحد کاری را به مخزن شبیه‌سازی‌شده وصل می‌کند.
    /// </summary>
    public CategoryServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Categories).Returns(_categoryRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new CategoryService(_unitOfWork.Object);
    }

    /// <summary>
    /// ایجاد دسته معتبر باید دسته‌ای با نام trim شده برگرداند و افزودن به مخزن و ذخیره‌سازی را انجام دهد.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedCategory()
    {
        const string name = "کالای دیجیتال";
        _categoryRepository.Setup(r => r.ExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var request = new CreateCategoryRequest
        {
            Name = "  کالای دیجیتال  ",
            Description = "محصولات دیجیتال",
            IsActive = true
        };

        CategoryDto result = await _service.CreateAsync(request);

        Assert.Equal(name, result.Name);
        Assert.Equal("محصولات دیجیتال", result.Description);
        Assert.True(result.IsActive);
        Assert.NotEqual(Guid.Empty, result.Id);
        _categoryRepository.Verify(
            r => r.AddAsync(
                It.Is<Category>(c => c.Name == name && c.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ایجاد دسته با نام تکراری باید DuplicateEntityException پرتاب کند و دسته‌ای اضافه نشود.
    /// </summary>
    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsDuplicate()
    {
        const string name = "پوشاک";
        _categoryRepository.Setup(r => r.ExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new CreateCategoryRequest { Name = name };

        await Assert.ThrowsAsync<DuplicateEntityException>(() => _service.CreateAsync(request));

        _categoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// به‌روزرسانی دسته با نامی که متعلق به دسته دیگری است باید DuplicateEntityException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsDuplicate()
    {
        Category category = CreateCategory("کتاب");
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _categoryRepository.Setup(r => r.ExistsByNameAsync("پوشاک", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new UpdateCategoryRequest { Name = "پوشاک", IsActive = true };

        await Assert.ThrowsAsync<DuplicateEntityException>(() => _service.UpdateAsync(category.Id, request));

        _categoryRepository.Verify(r => r.Update(It.IsAny<Category>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// حذف دسته‌ای که وجود ندارد باید EntityNotFoundException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Unknown_ThrowsNotFound()
    {
        Guid unknownId = Guid.NewGuid();
        _categoryRepository.Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(unknownId));

        _categoryRepository.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// صفحه‌بندی باید فیلترها را به مخزن پاس دهد و DTOهای درست را در نتیجه برگرداند.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_Filters_PassesThroughAndMaps()
    {
        Category first = CreateCategory("کالای دیجیتال");
        Category second = CreateCategory("پوشاک");
        var entities = new PagedEntities<Category>(new List<Category> { first, second }, totalCount: 2);

        _categoryRepository
            .Setup(r => r.GetPagedAsync("ک", true, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        var parameters = new PaginationParams { PageNumber = 1, PageSize = 10 };

        PagedResult<CategoryDto> page = await _service.GetPagedAsync(parameters, " ک ", isActive: true);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(first.Name, page.Items[0].Name);
        Assert.Equal(second.Name, page.Items[1].Name);
        _categoryRepository.Verify(
            r => r.GetPagedAsync("ک", true, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// یک دسته ساده برای تست‌ها می‌سازد.
    /// </summary>
    private static Category CreateCategory(string name)
    {
        DateTime now = DateTime.UtcNow;
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = null,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
