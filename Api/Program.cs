using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Extensions;
using OrderManagement.Api.Middleware;
using OrderManagement.Infrastructure.Extensions;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Persistence.Seed;
using Serilog;

// Bootstrap logger: active before the host is built, so configuration or DI failures
// during startup still end up in the console (the full Serilog pipeline takes over later).
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configuration sources: appsettings.json plus the environment-specific overlay
    // (appsettings.{Environment}.json) and environment variables, in that precedence order.
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // Persistence, repositories, password hasher and JWT token service (Infrastructure).
    builder.Services.AddInfrastructure(builder.Configuration);
    // MVC, Swagger, JWT bearer authentication, authorization and Serilog (Api).
    builder.Services.AddApiServices(builder.Configuration);
    // Application services consumed by the controllers.
    builder.Services.AddApplicationServices();

    WebApplication app = builder.Build();

    // Must run first: it turns any exception thrown further down the pipeline,
    // including authorization and controller failures, into a problem-details response.
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Structured request logging (method, path, status code, elapsed time) through Serilog.
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Order matters: the JWT bearer middleware authenticates the request and builds the
    // ClaimsPrincipal (including the mapped role claim) before authorization runs.
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Apply pending EF Core migrations and seed reference data. Failures are logged but
    // do not keep the host from starting, so the API stays reachable while the DB is down.
    await InitializeDatabaseAsync(app);

    // Fires once the server is listening; reports the addresses Kestrel actually bound.
    app.Services
        .GetRequiredService<IHostApplicationLifetime>()
        .ApplicationStarted.Register(() =>
            Log.Information(
                "Application started. Listening on: {ListeningUrls}",
                app.Urls.Count > 0 ? string.Join(", ", app.Urls) : "(default Kestrel endpoints)"));

    Log.Information("Starting Order Management API in {Environment} environment.", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Order Management API terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}

// Creates a DI scope, applies pending database migrations and runs the idempotent seeder.
// Both steps are wrapped in a try/catch so an unavailable database is reported through
// Serilog instead of crashing the host.
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        OrderManagementDbContext context = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(context, logger);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Database migration/seed failed during startup; the API will start anyway.");
    }
}
