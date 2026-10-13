namespace OrderManagement.Domain.Enums;

/// <summary>
/// Well-known role name constants used for authorization checks.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Role name for administrators. Administrators may cancel orders and manage the catalog.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Role name for regular users. Default role assigned at registration.
    /// </summary>
    public const string User = "User";
}
