using OrderManagement.Application.Common;
using Xunit;

namespace OrderManagement.Application.Tests;

/// <summary>
/// تست‌های واحد <see cref="PaginationParams"/> و محدودسازی مقادیر خارج از محدوده در <see cref="PaginationParams.Normalize"/>؛
/// بدون شبیه‌سازی و بدون پایگاه داده.
/// </summary>
public class PaginationParamsTests
{
    /// <summary>
    /// شماره‌ی صفحه‌ی فراتر از سقف باید به <see cref="PaginationParams.MaxPageNumber"/> محدود شود
    /// تا ضرب مرحله در اندازه‌ی صفحه سرریز نکند.
    /// </summary>
    [Fact]
    public void Normalize_PageNumberAboveMax_ClampsToMaxPageNumber()
    {
        var parameters = new PaginationParams { PageNumber = int.MaxValue };

        parameters.Normalize();

        Assert.Equal(PaginationParams.MaxPageNumber, parameters.PageNumber);
    }

    /// <summary>
    /// شماره‌ی صفحه‌ی کمتر از ۱ باید به ۱ محدود شود.
    /// </summary>
    [Fact]
    public void Normalize_PageNumberBelowMin_ClampsToOne()
    {
        var parameters = new PaginationParams { PageNumber = 0 };

        parameters.Normalize();

        Assert.Equal(1, parameters.PageNumber);
    }

    /// <summary>
    /// اندازه‌ی صفحه‌ی فراتر از سقف باید به <see cref="PaginationParams.MaxPageSize"/> محدود شود.
    /// </summary>
    [Fact]
    public void Normalize_PageSizeAboveMax_ClampsToMaxPageSize()
    {
        var parameters = new PaginationParams { PageSize = 500 };

        parameters.Normalize();

        Assert.Equal(PaginationParams.MaxPageSize, parameters.PageSize);
    }
}
