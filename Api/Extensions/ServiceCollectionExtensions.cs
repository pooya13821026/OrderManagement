using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using Serilog;

namespace OrderManagement.Api.Extensions;

/// <summary>
/// DI registration entry points for the API layer: MVC + Swagger, JWT bearer authentication,
/// Serilog and the application service registrations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MVC controllers, Swagger (with the Bearer authorize button and XML comments),
    /// JWT bearer authentication driven by the <c>Jwt</c> configuration section, authorization
    /// and Serilog request/structural logging. The JWT settings are read from the same
    /// section the <c>JwtTokenService</c> signs with, so issued and validated tokens always agree.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration; reads the <c>Jwt</c> section.</param>
    /// <returns>The service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when an argument is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <c>Jwt:Key</c> is missing or blank.</exception>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Order Management API",
                Version = "v1",
                Description = "Orders, customers and products API secured with JWT bearer authentication."
            });

            // Adds the "Authorize" button to Swagger UI: paste a token, every request
            // then carries the "Authorization: Bearer <token>" header.
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT access token. Enter the raw token; the 'Bearer ' prefix is added by Swagger UI."
            });

            // Adds the "Authorize" button to Swagger UI: paste a token, every request
            // then carries the "Authorization: Bearer <token>" header. The requirement is
            // resolved against the generated document so it points at the scheme above.
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
            });

            // Publish inline XML documentation when a project generates it; the Api always
            // does, the other layers only once GenerateDocumentationFile is enabled there.
            foreach (string fileName in new[] { "Api.xml", "Application.xml", "Domain.xml", "Infrastructure.xml" })
            {
                string path = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(path))
                {
                    options.IncludeXmlComments(path);
                }
            }
        });

        IConfigurationSection jwtSection = configuration.GetSection("Jwt");
        string issuer = jwtSection["Issuer"] ?? "OrderManagement";
        string audience = jwtSection["Audience"] ?? "OrderManagementClients";
        string key = jwtSection["Key"]
            ?? throw new InvalidOperationException("Missing required configuration value 'Jwt:Key'.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),

                    // The tokens carry the role in the short "role" claim; the bearer
                    // middleware maps it onto ClaimTypes.Role (inbound claim mapping), and
                    // RoleClaimType tells [Authorize(Roles = ...)] where to read roles from.
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization();

        // Serilog reads MinimumLevel/WriteTo (console + rolling file) from appsettings.json.
        services.AddSerilog((_, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(configuration));

        return services;
    }

    /// <summary>
    /// Registers the application services (scoped) that the controllers depend on.
    /// Their infrastructure dependencies (unit of work, repositories, token service) are
    /// registered separately by <c>AddInfrastructure</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
