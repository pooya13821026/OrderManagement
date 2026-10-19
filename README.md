# سرویس مدیریت سفارشات (Order Management Service)

API مدیریت سفارش‌های یک فروشگاه آنلاین با **Clean Architecture** چهارلایه، احراز هویت واقعی JWT،
قوانین تجاری کامل سفارش و سید خودکار داده. همه توضیحات کد (XML docs و کامنت‌ها) فارسی است.

## وضعیت تکمیل

همه بخش‌های تسک **تکمیل و اجراشده** است؛ هیچ بخش ناتمامی نیست:

| بخش تسک | وضعیت |
|---|---|
| ایجاد سفارش چندمحصولی (Order + OrderItems) | ✅ |
| جزئیات سفارش با شناسه | ✅ |
| جست‌وجو/فیلتر (CustomerId، Status، بازه زمانی) + پیجینیشن | ✅ |
| ترتیب وضعیت `Pending → Confirmed → Shipped → Delivered` | ✅ |
| حذف سفارش فقط Admin | ✅ |
| Bulk Insert سفارش‌ها (نتیجه تکی هر سفارش) | ✅ |
| مدل مشتری/محصول/موجودی + سید ۵۰ مشتری و ۲۰۰ محصول | ✅ (تأییدشده روی SQL Server) |
| دسته‌بندی محصول (Category CRUD + فیلتر CategoryId اختیاری) | ✅ |
| چک موجودی قبل از Confirm + جلوگیری از گذار غیرمجاز | ✅ |
| Clean Architecture چهارلایه (Domain، Application، Infrastructure، API) | ✅ |
| EF Core Code First + Migration + Seed | ✅ |
| JWT با نقش‌های Admin و User (ثبت‌نام/ورود/refresh/ابطال واقعی) | ✅ |
| AsNoTracking، تراکنش، Bulk بهینه | ✅ |
| Swagger + Postman Collection | ✅ |
| Unit Test سرویس‌ها و منطق کسب‌وکار (۷۰ تست سبز) | ✅ |

## معماری و تصمیمات طراحی

```
Domain (مرکز، بدون وابستگی)
  ← Application (فقط به Domain)
      ← Infrastructure (Application)
          ← Api (Infrastructure)
                ↖ Application.Tests (به Application)
```

- **Domain:** انتیتی‌ها (Customer، Product، Order، OrderItem، ApplicationUser، RefreshToken)،
  انام `OrderStatus`، استثناهای دامنه و قراردادهای ریپازیتوری. هیچ وابستگی ندارد.
- **Application:** DTOها، اینترفیس سرویس‌ها/`IUnitOfWork`، پیاده‌سازی سرویس‌ها
  (`OrderService`، `AuthService`، `CustomerService`، `ProductService`)، مپینگ و ولیدیشن.
  از EF و SQL هیچ‌چی نمی‌داند تا unit-testable بماند.
- **Infrastructure:** `DbContext` با FluentAPI، ریپازیتوری‌های EF (خواندن‌های نمایشی با
  `AsNoTracking`)، `UnitOfWork` با تراکنش، هش BCrypt، سرویس JWT و سید Bogus.
- **Api:** کنترلرها، JWT Bearer، Swagger، Serilog و میدلور خطای سراسری (ProblemDetails طبق RFC 7807).

**تصمیم‌های کلیدی:**
- گذار وضعیت فقط یک‌قدم به‌جلو + `Cancelled` از `Pending/Confirmed` فقط با Admin؛
  هنگام Confirm اول موجودی **همه** اقلام چک و بعد کم می‌شود (بدون کاهش نصفه).
- قیمت واحد در ردیف سفارش اسنپ‌شات می‌شود؛ تغییر بعدی قیمت کاتالوگ سفارش قدیمی را عوض نمی‌کند.
- لغو سفارشِ تأییدشده موجودی کسرشده را دقیقاً برمی‌گرداند (لغو از Pending چون چیزی کم نشده کاری ندارد).
- هر گذار وضعیت یک ردیف تاریخچه (از چی → به چی، کِی، توسط کی/چه نقشی، با چه دلیلی) در همان تراکنش ثبت می‌کند و زمان‌نشان مخصوص همان وضعیت (`ConfirmedAt/ShippedAt/...`) می‌خورد؛ جزئیات سفارش تایم‌لاین کامل را برمی‌گرداند.
- پس از commit هر گذار، اعلان (فعلاً لاگ ساخت‌یافته؛ پیاده‌سازی SMS/ایمیل واقعی بدون دست زدن به منطق وصل می‌شود) برای مشتری ثبت می‌شود.
- شماره سفارش یکتا با فرمت `ORD-yyyyMMdd-xxxxxx`؛ یکتایی نهایی با ایندکس دیتابیس.
- توکن refresh فقط به‌صورت هش SHA256 ذخیره و هنگام استفاده چرخش (rotation) می‌شود.
  ارائه‌ی توکن باطل‌شده کل زنجیره‌ی کاربر را باطل می‌کند (reuse detection).
- اندپوینت‌های ناشناس احراز هویت (register/login/refresh) محدودیت نرخ دارند (۲۰ درخواست در دقیقه برای هر IP).
- موجودی فقط از دو مسیر عوض می‌شود: Confirm سفارش (کاهش) و `POST /api/Products/AdjustStock/{id}` مخصوص Admin.
- مالکیت: ثبت‌نام هم‌زمان یک پروفایل مشتریِ متصل به حساب می‌سازد؛ کاربر عادی فقط سفارش‌های خودش را می‌بیند/جلو می‌برد و فقط برای مشتری خودش سفارش ثبت می‌کند؛ خواندن مشتریان و فهرست کاربران فقط Admin.
- Swagger در همه محیط‌ها فعال است تا ارزیابی مستندات راحت باشد
  (در محصول واقعی بهتر است فقط Development باشد).
- کلید JWT داخل `appsettings.json` فقط برای توسعه/ارزیابی است؛ در استقرار واقعی از
  user-secrets یا متغیر محیطی بدهید.

**محدودیت شناخته‌شده:** حذف مشتری/محصولی که سفارش مرتبط دارد به‌خاطر کلید خارجی رد می‌شود
(پاسخ ۵۰۰ عمومی). تسک فقط برای حذف سفارش قانون گذاشته بود، پس گارد اضافه نشد.

## پیش‌نیازها

- .NET 10 SDK
- SQL Server (برای اجرای لوکال: LocalDB؛ برای Docker: خودش بالا می‌آید)
- (اختیاری) Docker Desktop برای `docker compose`

## اجرای لوکال

```powershell
# ۱) بیلد
dotnet build OrderManagement.slnx

# ۲) اعمال مایگریشن (جدول‌ها را می‌سازد)
dotnet ef database update --project Infrastructure --startup-project Infrastructure

# ۳) اجرا (مایگریشن جاافتاده + سید خودکار هم هنگام بالا آمدن اجرا می‌شود)
dotnet run --project Api
```

- Swagger: `https://localhost:<port>/swagger` (پورت را از لاگ startup ببینید)
- لاگ فایل: `Api/logs/log-*.txt`
- رشته اتصال لوکال (در `Api/appsettings.json`): `Server=(localdb)\mssqllocaldb;Database=OrderManagementDb`

## اجرا با Docker

```powershell
docker compose up --build
```

- API روی `http://localhost:8080` (Swagger روی همان)
- SQL Server روی پورت 1433 (کاربر `sa`، رمز `Order@12345` — فقط توسعه)
- مایگریشن و سید (۲ کاربر، ۵۰ مشتری، ۸ دسته، ۲۰۰ محصول) موقع بالا آمدن خودکار اجرا می‌شود

## اجرای تست‌ها

```powershell
dotnet test OrderManagement.slnx
```

نتیجه موردانتظار: `Passed! - Failed: 0, Passed: 70, Skipped: 0` (تست‌های واحد خالص با xUnit و Moq، بدون دیتابیس).

## دریافت توکن JWT تستی

حساب‌های سیدشده (بعد از اولین اجرا/سید):

| نقش | ایمیل | رمز |
|---|---|---|
| Admin | `admin@ordermanagement.com` | `Admin@123` |
| User | `user@ordermanagement.com` | `User@123` |

```powershell
# ورود و گرفتن accessToken و refreshToken
$login = Invoke-RestMethod -Method Post http://localhost:8080/api/Auth/Login `
  -ContentType "application/json" `
  -Body '{"email":"admin@ordermanagement.com","password":"Admin@123"}'
$tok = $login.accessToken

# استفاده در ادامه (مثال: لیست سفارش‌ها)
Invoke-RestMethod http://localhost:8080/api/Orders/GetPaged -Headers @{ Authorization = "Bearer $tok" }
```

یا در Swagger دکمه **Authorize** و وارد کردن توکن؛ یا کالکشن Postman
(`postman/OrderManagement.postman_collection.json` + فایل environment) که لاگین/refresh
توکن‌ها را خودش ذخیره می‌کند. ثبت‌نام هم از `POST /api/Auth/Register` ممکن است
(نقش پیش‌فرض `User`).

## اندپوینت‌ها (خلاصه)

قرارداد مسیرها اکشن‌محور است: `api/{کنترلر}/{اکشن}` (مثلاً `api/Orders/Create`).
قرارداد متدها: خواندن‌ها `GET` و همه‌ی نوشتن‌ها `POST` (ویرایش، تغییر وضعیت، شارژ موجودی و حذف هم POST هستند).

| متد | مسیر | دسترسی |
|---|---|---|
| POST | `/api/Auth/Register` ،`/Login` ،`/Refresh` | آزاد |
| POST | `/api/Auth/Revoke` — GET `/api/Auth/Me` (شامل `customerId` متصل) | احرازشده |
| GET `/api/Users/GetPaged` | فقط Admin |
| POST `/api/Orders/Create` — GET `/api/Orders/GetPaged` (فیلتر+پیج) — GET `/api/Orders/GetById/{id}` | کاربر عادی فقط مال خودش، مدیر همه |
| POST `/api/Orders/UpdateStatus/{id}` (`{newStatus}` + دلیل اختیاری) | مال خودش (لغو فقط Admin) |
| POST `/api/Orders/Delete/{id}` — POST `/api/Orders/BulkCreate` | Admin |
| GET+POST `/api/Customers/...` (همه اکشن‌ها) | فقط Admin |
| GET `/api/Categories/GetPaged` ،`/GetById/{id}` — POST `/Create` ،`/Update/{id}` ،`/Delete/{id}` | خواندن احرازشده، نوشتن Admin |
| GET `/api/Products/GetPaged` ،`/GetById/{id}` — POST `/Create` ،`/Update/{id}` ،`/Delete/{id}` | خواندن احرازشده، نوشتن Admin |
| POST `/api/Products/AdjustStock/{id}` (`{quantityChange}`) | Admin |

## ساختار مخزن

```
Domain/            انتیتی‌ها، انام‌ها، استثناها، قراردادهای ریپازیتوری
Application/       DTOها، اینترفیس‌ها، سرویس‌ها، مپینگ، ولیدیشن
Infrastructure/    DbContext، کانفیگ‌ها، ریپازیتوری‌ها، UnitOfWork، امنیت، سید، مایگریشن‌ها
Api/               کنترلرها، میدلور، تنظیمات DI، appsettings، Dockerfile
Application.Tests/ ۷۰ تست واحد (OrderService، AuthService، ProductService، CategoryService)
postman/           کالکشن ۲۸ درخواستی + environment
docker-compose.yml SQL Server 2022 + API
```

## تاریخچه کامیت‌ها

هر بخش با کامیت معنادار جداگانه ثبت شده (`git log --oneline`):
دامنه → زیرساخت → API → مایگریشن → فارسی‌سازی → ریویو/رفکتور → تست → Docker/Postman.
