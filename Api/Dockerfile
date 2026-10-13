# ============================================================================
# Dockerfile چندمرحله‌ای برای Order Management API (.NET 10)
# - مرحله build: تصویر SDK برای بازیابی NuGet و publish در حالت Release
# - مرحله runtime: تصویر aspnet با کاربر غیر root (APP_UID)
#
# نکته: context باید ریشه‌ی مخزن باشد (در docker-compose: context: .
# و dockerfile: Api/Dockerfile) چون Api.csproj به پروژه‌های خواهر
# Application/Infrastructure/Domain ارجاع دارد و خارج از context نسخه‌برداری نمی‌شوند.
# ============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# ابتدا فقط فایل‌های csproj تا لایه‌ی بازیابی NuGet با تغییر سورس‌ها کش بماند.
COPY Api/Api.csproj Api/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Domain/Domain.csproj Domain/
RUN dotnet restore Api/Api.csproj

# بقیه‌ی سورس‌ها و سپس publish پروژه‌ی Api در حالت Release.
COPY Api/ Api/
COPY Application/ Application/
COPY Infrastructure/ Infrastructure/
COPY Domain/ Domain/
RUN dotnet publish Api/Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Serilog فایل‌های لاگ را در پوشه‌ی logs/ کنار برنامه می‌سازد؛ چون برنامه با
# کاربر غیر root اجرا می‌شود، مالکیت این پوشه از قبل به کاربر runtime داده
# می‌شود تا نوشتن لاگ در حین اجرا با خطای دسترسی مواجه نشود.
RUN mkdir -p /app/logs && chown $APP_UID /app/logs

# Kestrel داخل کانتینر روی پورت 8080 گوش می‌دهد (پورت غیر privileged).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Api.dll"]
