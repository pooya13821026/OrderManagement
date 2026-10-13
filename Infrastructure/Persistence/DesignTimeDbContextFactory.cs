using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used ONLY by the EF Core CLI tools
/// (<c>dotnet ef migrations ...</c>) so the tools can create the
/// <see cref="OrderManagementDbContext"/> without running <c>Api/Program.cs</c>
/// (which would start the web host, Serilog and the seeder).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderManagementDbContext>
{
    /// <summary>
    /// Creates the context for design-time operations using the API project's
    /// configuration when available, falling back to LocalDB otherwise.
    /// </summary>
    /// <param name="args">Command-line arguments passed by the EF tools.</param>
    /// <returns>A configured context instance.</returns>
    public OrderManagementDbContext CreateDbContext(string[] args)
    {
        // The factory assembly is Infrastructure/bin; walk up to the solution root,
        // then into Api/ so migrations pick up the same connection string as runtime.
        string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Api");

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(basePath) ? basePath : Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=OrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new OrderManagementDbContext(options);
    }
}
