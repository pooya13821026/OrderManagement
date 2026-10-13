using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Security;

namespace OrderManagement.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent database seeder: on an empty database it creates the two bootstrap accounts
/// (one per role), exactly 50 customers and 200 products with a deterministic Bogus seed,
/// and on any later run it only tops missing rows up. No migrations are applied here -
/// that is the caller's responsibility.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Number of customers the seed guarantees.
    /// </summary>
    public const int CustomerSeedCount = 50;

    /// <summary>
    /// Number of products the seed guarantees.
    /// </summary>
    public const int ProductSeedCount = 200;

    /// <summary>
    /// Fixed Bogus seed so every run generates the same sample data.
    /// </summary>
    private const int BogusSeed = 12345;

    /// <summary>Sign-in address of the seeded administrator.</summary>
    private const string AdminEmail = "admin@ordermanagement.com";

    /// <summary>Sign-in password of the seeded administrator (development seed data).</summary>
    private const string AdminPassword = "Admin@123";

    /// <summary>Sign-in address of the seeded regular user.</summary>
    private const string UserEmail = "user@ordermanagement.com";

    /// <summary>Sign-in password of the seeded regular user (development seed data).</summary>
    private const string UserPassword = "User@123";

    /// <summary>
    /// Seeds users, customers and products idempotently. Existing rows are never touched;
    /// a partially seeded database is topped up to exactly 50 customers and 200 products.
    /// </summary>
    /// <param name="context">The database context to seed.</param>
    /// <param name="logger">Logger used to report the resulting row counts.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is <c>null</c>.</exception>
    public static async Task SeedAsync(
        OrderManagementDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        int createdUsers = await SeedUsersAsync(context, cancellationToken);
        int createdCustomers = await SeedCustomersAsync(context, cancellationToken);
        int createdProducts = await SeedProductsAsync(context, cancellationToken);

        // A single save keeps the whole seed atomic: either all rows land or none do.
        await context.SaveChangesAsync(cancellationToken);

        int userCount = await context.Users.CountAsync(cancellationToken);
        int customerCount = await context.Customers.CountAsync(cancellationToken);
        int productCount = await context.Products.CountAsync(cancellationToken);

        // Roles are plain strings on the user row (the domain model has no roles table),
        // so "creating roles" means ensuring one seeded account per well-known role.
        logger.LogInformation(
            "Seed finished: roles [{Roles}]; {UserCount} users ({CreatedUsers} created), " +
            "{CustomerCount} customers ({CreatedCustomers} created), {ProductCount} products ({CreatedProducts} created).",
            string.Join(", ", new[] { Roles.Admin, Roles.User }),
            userCount,
            createdUsers,
            customerCount,
            createdCustomers,
            productCount,
            createdProducts);
    }

    /// <summary>
    /// Creates one account for each well-known role (admin with <see cref="Roles.Admin"/>,
    /// regular user with <see cref="Roles.User"/>) unless it already exists.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of users that were created.</returns>
    private static async Task<int> SeedUsersAsync(OrderManagementDbContext context, CancellationToken cancellationToken)
    {
        var passwordHasher = new BCryptPasswordHasher();
        int created = 0;

        if (!await context.Users.AnyAsync(user => user.Email == AdminEmail, cancellationToken))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = AdminEmail,
                PasswordHash = passwordHasher.Hash(AdminPassword),
                FirstName = "Admin",
                LastName = "Admin",
                IsActive = true,
                Role = Roles.Admin,
                SecurityStamp = Guid.NewGuid().ToString("N")
            });
            created++;
        }

        if (!await context.Users.AnyAsync(user => user.Email == UserEmail, cancellationToken))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "user",
                Email = UserEmail,
                PasswordHash = passwordHasher.Hash(UserPassword),
                FirstName = "Demo",
                LastName = "User",
                IsActive = true,
                Role = Roles.User,
                SecurityStamp = Guid.NewGuid().ToString("N")
            });
            created++;
        }

        return created;
    }

    /// <summary>
    /// Tops the customer table up to <see cref="CustomerSeedCount"/> rows using a
    /// deterministic Bogus sequence (seed 12345). Emails embed the row number, so they
    /// stay unique even when a partial seed is completed by a later run.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of customers that were created.</returns>
    private static async Task<int> SeedCustomersAsync(OrderManagementDbContext context, CancellationToken cancellationToken)
    {
        int existingCount = await context.Customers.CountAsync(cancellationToken);
        int missingCount = CustomerSeedCount - existingCount;
        if (missingCount <= 0)
        {
            return 0;
        }

        var customerFaker = new Faker<Customer>("en")
            .UseSeed(BogusSeed)
            .RuleFor(customer => customer.FirstName, faker => faker.Name.FirstName())
            .RuleFor(customer => customer.LastName, faker => faker.Name.LastName())
            .RuleFor(customer => customer.Phone, faker => faker.Phone.PhoneNumber())
            .RuleFor(customer => customer.Address, faker => faker.Address.FullAddress());

        for (int index = existingCount; index < CustomerSeedCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Customer customer = customerFaker.Generate();
            customer.Id = Guid.NewGuid();
            // Row numbers make the generated addresses unique across all runs.
            customer.Email = BuildEmail(customer.FirstName, customer.LastName, index + 1);
            context.Customers.Add(customer);
        }

        return missingCount;
    }

    /// <summary>
    /// Tops the product table up to <see cref="ProductSeedCount"/> rows using a
    /// deterministic Bogus sequence (seed 12345). SKUs follow the unique <c>SKU-0001</c>
    /// scheme and every 10th product (exactly 10%) is inactive.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of products that were created.</returns>
    private static async Task<int> SeedProductsAsync(OrderManagementDbContext context, CancellationToken cancellationToken)
    {
        int existingCount = await context.Products.CountAsync(cancellationToken);
        int missingCount = ProductSeedCount - existingCount;
        if (missingCount <= 0)
        {
            return 0;
        }

        var productFaker = new Faker<Product>("en")
            .UseSeed(BogusSeed)
            .RuleFor(product => product.Name, faker => faker.Commerce.ProductName())
            .RuleFor(product => product.Description, faker => faker.Commerce.ProductDescription())
            .RuleFor(product => product.Price, faker => Math.Round(faker.Random.Decimal(1m, 1000m), 2))
            .RuleFor(product => product.StockQuantity, faker => faker.Random.Int(0, 500));

        for (int index = existingCount; index < ProductSeedCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Product product = productFaker.Generate();
            product.Id = Guid.NewGuid();
            int rowNumber = index + 1;
            product.Sku = $"SKU-{rowNumber:D4}";
            // Deterministic 10% inactive share: every 10th product is retired.
            product.IsActive = rowNumber % 10 != 0;
            context.Products.Add(product);
        }

        return missingCount;
    }

    /// <summary>
    /// Builds a unique, mail-box-safe sample address from a name and a row number,
    /// for example <c>john.smith.001@example.com</c>.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="rowNumber">The 1-based row number.</param>
    /// <returns>The generated email address.</returns>
    private static string BuildEmail(string firstName, string lastName, int rowNumber)
    {
        string localPart = SanitizeEmailPart($"{firstName}.{lastName}");
        return $"{localPart}.{rowNumber:D3}@example.com";
    }

    /// <summary>
    /// Lowercases the value and keeps only ASCII letters, digits and dots, so generated
    /// names (for example <c>O'Brien</c>) can never produce an invalid address.
    /// </summary>
    /// <param name="value">The raw value.</param>
    /// <returns>The sanitized value, or <c>customer</c> when nothing usable remains.</returns>
    private static string SanitizeEmailPart(string value)
    {
        string lowered = value.ToLowerInvariant();
        char[] cleaned = lowered
            .Where(character =>
                character is >= 'a' and <= 'z'
                or >= '0' and <= '9'
                or '.')
            .ToArray();

        string result = new(cleaned);
        return string.IsNullOrEmpty(result) ? "customer" : result;
    }
}
