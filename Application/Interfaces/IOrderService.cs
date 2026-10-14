using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد سرویس برنامه برای عملیات چرخه‌ی حیات سفارش.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// سفارش جدیدی را با وضعیت <see cref="OrderStatus.Pending"/> پس از اعتبارسنجی
    /// مشتری، محصولات سفارش‌داده‌شده و تعدادها ایجاد می‌کند، از قیمت‌های واحد اسنپ‌شات می‌گیرد
    /// و قیمت کل ردیف‌ها و مبلغ کل سفارش را محاسبه می‌کند.
    /// فراخواننده‌ی غیرمدیر فقط می‌تواند برای پروفایل مشتریِ متصل به حساب خودش سفارش ثبت کند؛
    /// مدیر می‌تواند برای هر مشتری معتبری سفارش بسازد.
    /// </summary>
    /// <param name="request">بدنه‌ی درخواست ایجاد سفارش.</param>
    /// <param name="callerUserId">شناسه‌ی کاربر فراخواننده (از claims لایه‌ی API).</param>
    /// <param name="callerRole">نقش کاربر فراخواننده (از claims لایه‌ی API).</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>سفارش ایجادشده به‌صورت DTO.</returns>
    /// <exception cref="Domain.Exceptions.DomainException">وقتی کاربرِ غیرمدیر پروفایل مشتریِ متصل نداشته باشد پرتاب می‌شود.</exception>
    /// <exception cref="Domain.Exceptions.UnauthorizedOperationException">وقتی غیرمدیر مشتریِ غیر از پروفایل خودش را بخواهد پرتاب می‌شود.</exception>
    Task<OrderDto> CreateAsync(
        CreateOrderRequest request,
        Guid callerUserId,
        string callerRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// یک سفارش را بر اساس شناسه، شامل مشتری و اقلام، برمی‌گرداند.
    /// فراخواننده‌ی غیرمدیر فقط سفارش‌هایی را می‌بیند که مشتری‌اش به حساب او متصل است؛
    /// مدیر همه‌ی سفارش‌ها را می‌بیند.
    /// </summary>
    /// <param name="id">شناسه‌ی سفارش.</param>
    /// <param name="callerUserId">شناسه‌ی کاربر فراخواننده؛ ناموجود <c>null</c>.</param>
    /// <param name="callerRole">نقش کاربر فراخواننده (از claims لایه‌ی API).</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>سفارش به‌صورت DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی سفارش وجود نداشته باشد پرتاب می‌شود.</exception>
    /// <exception cref="Domain.Exceptions.UnauthorizedOperationException">وقتی غیرمدیر سفارشی غیر از سفارش‌های خودش را بخواهد پرتاب می‌شود.</exception>
    Task<OrderDto> GetByIdAsync(
        Guid id,
        Guid? callerUserId,
        string callerRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای فیلترشده، مرتب‌شده و صفحه‌بندی‌شده از سفارش‌ها برمی‌گرداند.
    /// برای فراخواننده‌ی غیرمدیر، فیلتر مشتری همیشه به پروفایل متصل به حساب او بازنویسی
    /// می‌شود (بازنویسی فیلترِ ارسالی)؛ اگر حساب پروفایلی نداشته باشد صفحه‌ی خالی برمی‌گردد.
    /// مدیر فیلتر دست‌نخورده می‌گیرد.
    /// </summary>
    /// <param name="filter">ورودی‌های فیلتر، مرتب‌سازی و صفحه‌بندی.</param>
    /// <param name="callerUserId">شناسه‌ی کاربر فراخواننده؛ ناموجود <c>null</c>.</param>
    /// <param name="callerRole">نقش کاربر فراخواننده (از claims لایه‌ی API).</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>صفحه‌ای از DTOهای سفارش.</returns>
    Task<PagedResult<OrderDto>> GetPagedAsync(
        OrderFilterParams filter,
        Guid? callerUserId,
        string callerRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// سفارش را به وضعیت جدیدی می‌برد و قواعد گذار رو به جلو را اعمال می‌کند.
    /// هنگام تأیید، ابتدا موجودی همه‌ی اقلام اعتبارسنجی و سپس کاهش داده می‌شود؛
    /// لغو از <c>Confirmed</c> موجودی اقلام را برمی‌گرداند و لغو نیازمند نقش <c>Admin</c> است.
    /// علاوه بر آن فراخواننده‌ی غیرمدیر فقط می‌تواند سفارشِ متصل به حساب خودش را جلو ببرد؛
    /// برای غیرمدیر بدون شناسه (<paramref name="changedByUserId"/> ناموجود) مالکیت اثبات
    /// نمی‌شود و خطای ۴۰۳ می‌گیرد. هر گذار در همان تراکنش، زمان‌نشان وضعیت و یک ردیف
    /// تاریخچه ثبت می‌کند و پس از commit اطلاع‌رسانی (لاگ) انجام می‌شود.
    /// </summary>
    /// <param name="orderId">شناسه‌ی سفارش.</param>
    /// <param name="newStatus">وضعیت جدید درخواستی.</param>
    /// <param name="requesterRole">نقش کاربر فراخواننده (از claims لایه‌ی API).</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <param name="reason">دلیل اختیاری تغییر (حداکثر ۵۰۰ نویسه)؛ فقط برای لغو معنا دارد.</param>
    /// <param name="changedByUserId">شناسه‌ی کاربر فراخواننده برای مالکیت و ثبت در تاریخچه؛ ناموجود <c>null</c>.</param>
    /// <returns>سفارش به‌روزرسانی‌شده به‌صورت DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی سفارش وجود نداشته باشد پرتاب می‌شود.</exception>
    /// <exception cref="Domain.Exceptions.UnauthorizedOperationException">وقتی فراخواننده مالک سفارش نباشد (یا غیرمدیر سفارش را لغو کند) پرتاب می‌شود.</exception>
    Task<OrderDto> UpdateStatusAsync(
        Guid orderId,
        OrderStatus newStatus,
        string requesterRole,
        CancellationToken cancellationToken = default,
        string? reason = null,
        Guid? changedByUserId = null);

    /// <summary>
    /// پس از بررسی وجود سفارش، آن را حذف می‌کند. بررسی نقش‌ها توسط لایه‌ی API اعمال می‌شود.
    /// </summary>
    /// <param name="id">شناسه‌ی سفارش.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">وقتی سفارش وجود نداشته باشد پرتاب می‌شود.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// چند سفارش را در یک تراکنش ایجاد می‌کند و نتیجه‌ی موفقیت یا شکست هر سفارش را ثبت می‌کند
    /// تا شکست‌های جزئی به جای نادیده گرفته شدن، گزارش شوند.
    /// </summary>
    /// <param name="request">بدنه‌ی ایجاد گروهی (حداکثر ۵۰۰ سفارش).</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>پاسخ گروهی شامل نتایج هر سفارش.</returns>
    Task<BulkCreateOrdersResponse> BulkCreateAsync(
        BulkCreateOrdersRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تعیین می‌کند که آیا گذار وضعیت درخواستی توسط قواعد کسب‌وکار مجاز است:
    /// فقط رو به جلو و تک‌مرحله‌ای <c>Pending → Confirmed → Shipped → Delivered</c>،
    /// به‌علاوه‌ی <c>Cancelled</c> از <c>Pending</c> یا <c>Confirmed</c>.
    /// </summary>
    /// <param name="currentStatus">وضعیت فعلی سفارش.</param>
    /// <param name="requestedStatus">وضعیت درخواست‌شده.</param>
    /// <returns><c>true</c> اگر گذار مجاز باشد؛ در غیر این صورت <c>false</c>.</returns>
    bool CanTransition(OrderStatus currentStatus, OrderStatus requestedStatus);
}
