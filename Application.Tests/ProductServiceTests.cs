using Moq;
using OrderManagement.Application.Dtos.Products;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Repositories;
using Xunit;

namespace OrderManagement.Application.Tests;

/// <summary>
/// تست‌های واحد <see cref="ProductService"/> با شبیه‌سازی واحد کاری و مخزن محصولات؛
/// بدون EF Core، بدون پایگاه داده و بدون شبکه.
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly ProductService _service;

    /// <summary>
    /// نمونه‌ای جدید می‌سازد و مخزن محصولات واحد کاری را به مخزن شبیه‌سازی‌شده وصل می‌کند.
    /// </summary>
    public ProductServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Products).Returns(_productRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new ProductService(_unitOfWork.Object);
    }

    /// <summary>
    /// افزایش موجودی با تغییر مثبت باید موجودی را به اندازه‌ی تغییر زیاد کند و محصول را ذخیره کند.
    /// </summary>
    [Fact]
    public async Task AdjustStockAsync_PositiveChange_Increases()
    {
        Product product = CreateProduct(stockQuantity: 5);
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        ProductDto result = await _service.AdjustStockAsync(product.Id, 3);

        Assert.Equal(8, product.StockQuantity);
        Assert.Equal(8, result.StockQuantity);
        _productRepository.Verify(r => r.Update(product), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// برداشت بیش از حد موجودی باید InsufficientStockException پرتاب کند و موجودی دست‌نخورده بماند.
    /// </summary>
    [Fact]
    public async Task AdjustStockAsync_Overdraw_ThrowsInsufficientStock()
    {
        Product product = CreateProduct(stockQuantity: 5);
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        InsufficientStockException exception = await Assert.ThrowsAsync<InsufficientStockException>(
            () => _service.AdjustStockAsync(product.Id, -10));

        Assert.Equal(product.Id, exception.ProductId);
        Assert.Equal(5, exception.Available);
        Assert.Equal(10, exception.Requested);
        Assert.Equal(5, product.StockQuantity);
        _productRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// تغییر صفر موجودی باید DomainException پرتاب کند و حتی محصول بارگذاری نشود.
    /// </summary>
    [Fact]
    public async Task AdjustStockAsync_ZeroChange_ThrowsDomain()
    {
        await Assert.ThrowsAsync<DomainException>(() => _service.AdjustStockAsync(Guid.NewGuid(), 0));

        _productRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ایجاد محصول با SKU تکراری باید DuplicateEntityException پرتاب کند و محصولی اضافه نشود.
    /// </summary>
    [Fact]
    public async Task CreateAsync_DuplicateSku_ThrowsDuplicate()
    {
        const string sku = "SKU-DUPLICATE-1";
        _productRepository.Setup(r => r.ExistsBySkuAsync(sku, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new CreateProductRequest
        {
            Name = "Laptop",
            Sku = sku,
            Price = 1000m,
            StockQuantity = 4
        };

        await Assert.ThrowsAsync<DuplicateEntityException>(() => _service.CreateAsync(request));

        _productRepository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// یک محصول ساده برای تست‌ها می‌سازد.
    /// </summary>
    private static Product CreateProduct(int stockQuantity)
    {
        DateTime now = DateTime.UtcNow;
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop",
            Sku = $"SKU-{Guid.NewGuid():N}",
            Price = 100m,
            StockQuantity = stockQuantity,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
